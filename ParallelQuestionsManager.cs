using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stacky;
using System.Threading.Tasks;
using System.Threading;

namespace Newest_unaswered_by_tags
{
	class ParallelQuestionsManager : QuestionsManager
	{
		static readonly TimeSpan RefreshPeriod = TimeSpan.FromMinutes(1);

		Task lastScheduledTask;
		CancellationTokenSource tokenSource = new CancellationTokenSource();
		Timer refreshTimer;

		public ParallelQuestionsManager(Site site, IEnumerable<string> tags)
			: base(site, tags)
		{
			refreshTimer = new Timer(_ => scheduleTask(processQuestions), null, TimeSpan.Zero, RefreshPeriod);
		}

		void scheduleTask(Action action)
		{
			if (lastScheduledTask == null)
				lastScheduledTask = Task.Factory.StartNew(action, tokenSource.Token);
			else
				lastScheduledTask = lastScheduledTask.ContinueWith(_ => action(), tokenSource.Token);
		}

		void cancelTasks()
		{
			tokenSource.Cancel();
			tokenSource = new CancellationTokenSource();
		}

		public override Site Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				if (Site != value)
				{
					cancelTasks();
					scheduleTask(() => base.Site = value);
				}
			}
		}

		public override IEnumerable<string> Tags
		{
			get
			{
				return base.Tags;
			}
			set
			{
				if (Tags == null || value == null || Tags.Count() != value.Count() || Tags.Zip(value, (tag1, tag2) => tag1 == tag2).Any(b => !b))
				{
					cancelTasks();
					scheduleTask(() => base.Tags = value);
				}
			}
		}

		public override void Remove(Question question)
		{
			scheduleTask(() => base.Remove(question));
		}

		public override void Remove(IEnumerable<Question> questionsToRemove)
		{
			scheduleTask(() => base.Remove(questionsToRemove));
		}

		protected override void appendQuestions()
		{
			if (appendQuestion())
				scheduleTask(appendQuestions);
		}
	}
}