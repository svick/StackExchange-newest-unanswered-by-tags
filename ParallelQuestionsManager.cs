using System;
using System.Collections.Generic;
using System.Linq;
using Stacky;
using System.Threading.Tasks;
using System.Threading;

namespace Newest_unaswered_by_tags
{
	class ParallelQuestionsManager : QuestionsManager
	{
		static readonly TimeSpan RefreshPeriod = TimeSpan.FromMinutes(1);

		Task m_lastScheduledTask;
		CancellationTokenSource m_tokenSource = new CancellationTokenSource();
		Timer m_refreshTimer;

		public ParallelQuestionsManager(Site site, IEnumerable<string> tags)
			: base(site, tags)
		{
			m_refreshTimer = new Timer(_ => ScheduleTask(ProcessQuestions), null, TimeSpan.Zero, RefreshPeriod);
		}

		void ScheduleTask(Action action)
		{
			if (m_lastScheduledTask == null)
				m_lastScheduledTask = Task.Factory.StartNew(action, m_tokenSource.Token);
			else
				m_lastScheduledTask = m_lastScheduledTask.ContinueWith(_ => action(), m_tokenSource.Token);

			m_lastScheduledTask = m_lastScheduledTask.ContinueWith(HandleExceptions, TaskContinuationOptions.OnlyOnFaulted);
		}

		void HandleExceptions(Task task)
		{
			ShowException(task.Exception.InnerExceptions[0]);
		}

		void CancelTasks()
		{
			m_tokenSource.Cancel();
			m_tokenSource = new CancellationTokenSource();
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
					CancelTasks();
					ScheduleTask(() => base.Site = value);
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
					CancelTasks();
					ScheduleTask(() => base.Tags = value);
				}
			}
		}

		protected override void AppendQuestions()
		{
		    ScheduleTask(() =>
		                 {
                             if (AppendQuestion())
                                 AppendQuestions();
		                 });
		}
	}
}