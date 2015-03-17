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
			string owner = "mono";
			string repo = "mono";

			string input;
			int months;
			do {
				Console.Write ("Enter the number of months that make a PR stale: ");
				input = Console.ReadLine ();
			} while (!int.TryParse (input, out months));

			var conn = new Connection (new ProductHeaderValue ("DeadPRBumper", "1.0"));
			var client = new GitHubClient (conn);

			// Note: This means that the last push, comment, diff comment, whatever, happened two months ago.
			// Filtering by specific statuses (i.e. commits) is doable.
			var prs = client.PullRequest.GetForRepository (owner, repo).Result
				.Where (pr => pr.UpdatedAt.AddMonths (months) < DateTimeOffset.UtcNow)
				.ToList ();

			foreach (var pr in prs)
				Console.WriteLine ("{0} - {1} days", pr.HtmlUrl, (DateTimeOffset.UtcNow - pr.UpdatedAt).Days);

			if (user == null || pass == null)
				return;

			// do prompt.
		}
	}
}
