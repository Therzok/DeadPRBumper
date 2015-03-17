using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;
using System.Threading.Tasks;

namespace DeadPRBumper
{
	class MainClass
	{
		// TODO: Bump people with rebase if non-mergeable. Probably heuristic to check if someone asked.
		// TODO: Filter who did last update. Someone from mono org or someone from outside.
		// TODO: Add per PR prompt.
		public static void Main (string[] args)
		{
			string user = null; // Enter your GitHub username.
			string pass = null; // Enter your GitHub pass/2FA token.
			const string bumpMessage = "Any update on this?";
			const string rebaseMessage = "This needs a rebase on top of the target branch.";
			const string owner = "Therzok";
			const string repo = "DeadPRBumper";

			string input;
			int months;
			do {
				Console.Write ("Enter the number of months that make a PR stale: ");
				input = Console.ReadLine ();
			} while (!int.TryParse (input, out months));

			var conn = new Connection (new ProductHeaderValue ("DeadPRBumper", "1.0"));
			if (!string.IsNullOrEmpty (user) && !string.IsNullOrEmpty (pass))
				conn.Credentials = new Credentials (user, pass);

			var client = new GitHubClient (conn);

			// Note: This means that the last push, comment, diff comment, whatever, happened two months ago.
			// Filtering by specific statuses (i.e. commits) is doable.
			var prs = client.PullRequest.GetForRepository (owner, repo).Result
				.Where (pr => pr.UpdatedAt.AddMonths (months) < DateTimeOffset.UtcNow)
				.ToList ();

			foreach (var pr in prs) {
				Console.WriteLine ("{0} - {1} days", pr.HtmlUrl, (DateTimeOffset.UtcNow - pr.UpdatedAt).Days);
			}

			if (user == null || pass == null)
				return;

			Console.WriteLine ("Bump the PRs? y/n");
			input = Console.ReadLine ();
			if (input != "y")
				return;

			foreach (var pr in prs) {
				var issueComment = client.Issue.Comment.Create (owner, repo, pr.Number, /*!pr.Mergeable.GetValueOrDefault () ? rebaseMessage :*/ bumpMessage).Result;
				// Do stuff with it?
			}
			Console.ReadKey ();
		}
	}
}
