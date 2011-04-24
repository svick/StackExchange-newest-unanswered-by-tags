using System;
using System.Collections.Generic;
using System.Linq;
using Stacky;

namespace Newest_unaswered_by_tags
{
	public class ExceptionEventArgs : EventArgs
	{
		public Exception Exception
		{
			get;
			protected set;
		}

		public ExceptionEventArgs(Exception exception)
		{
			Exception = exception;
		}
	}

	class QuestionsManager
	{
		static readonly int PageSize = 100;

	    readonly LinkedList<Question> m_questions = new LinkedList<Question>();
		public virtual IEnumerable<Question> Questions
		{
			get
			{
				return m_questions;
			}
		}

		public int QuestionsToLoad
		{
			get;
			set;
		}

		StackyClient m_client;
		Site m_site;
		public virtual Site Site
		{
			get
			{
				return m_site;
			}
			set
			{
				SetSite(value);
				Reset();
			}
		}

		void SetSite(Site value)
		{
			m_site = value;
			m_client = new StackyClient("1.0", "P2aPrA4wOEiskh8pVm1UKg", m_site, new UrlClient(), new JsonProtocol());
		}

		HashSet<string> m_tags;
		public virtual IEnumerable<string> Tags
		{
			get
			{
				return m_tags;
			}
			set
			{
				SetTags(value);
				Reset();
			}
		}

		void SetTags(IEnumerable<string> value)
		{
			m_tags = new HashSet<string>(value);
		}

		public event EventHandler QuestionsChanged = delegate { };
		public event EventHandler<ExceptionEventArgs> ExceptionOcurred = delegate { };

		public QuestionsManager(Site site, IEnumerable<string> tags)
		{
			QuestionsToLoad = 10;
			SetSite(site);
			SetTags(tags);
		}

		//what if we reach end?
		IEnumerable<Question> LoadQuestions()
		{
			int page = 1;
			while (true)
			{
				IEnumerable<Question> questions = m_client.GetQuestions(
					sortBy: QuestionSort.UnansweredCreation,
					pageSize: PageSize,
					page: page++)
					.Where(q => !m_questionsToIgnore.Contains(q.Id));

				if (Tags != null && Tags.Any())
					questions = questions.Where(q => q.Tags.Any(tag => m_tags.Contains(tag)));

				foreach (Question question in questions)
					yield return question;
			}
		}

		IEnumerator<Question> m_incoming = null;

		//expects that loadQuestions() returns an "infinite" sequence
		protected void ProcessQuestions()
		{
			m_incoming = LoadQuestions().GetEnumerator();
			m_incoming.MoveNext();

			if (m_questions.Any())
			{
				var current = m_questions.First;
				while (current != null)
				{
					if (m_incoming.Current.Id > current.Value.Id)
					{
						m_questions.AddBefore(current, m_incoming.Current);
						m_incoming.MoveNext();
					}
					else if (m_incoming.Current.Id < current.Value.Id)
					{
						var toDelete = current;
						current = current.Next;
					    try
					    {
					        m_questions.Remove(toDelete);
					    }
					    catch (InvalidOperationException)
					    {
					        // question must have been deleted already
					    }
					}
					else
					{
						current.Value = m_incoming.Current;
						current = current.Next;
						m_incoming.MoveNext();
					}
				}
			}

			AppendQuestions();
		}

		protected virtual void AppendQuestions()
		{
			while (AppendQuestion())
				;
		}

		protected bool AppendQuestion()
		{
			if (m_questions.Count < QuestionsToLoad)
			{
                if (!m_questions.Any(q => q.Id == m_incoming.Current.Id))
                    m_questions.AddLast(m_incoming.Current);
				m_incoming.MoveNext();
			}

			QuestionsChanged(this, EventArgs.Empty);

			return m_questions.Count < QuestionsToLoad;
		}

		void Reset()
		{
			m_questions.Clear();

			if (m_client != null)
			{
				m_incoming = LoadQuestions().GetEnumerator();
				m_incoming.MoveNext();
				AppendQuestions();
			}
		}

	    readonly HashSet<int> m_questionsToIgnore = new HashSet<int>();

		public virtual void Remove(Question question)
		{
			m_questionsToIgnore.Add(question.Id);
			m_questions.Remove(question);
			QuestionsChanged(this, EventArgs.Empty);
			AppendQuestions();
		}

		public virtual void Remove(IEnumerable<Question> questionsToRemove)
		{
			foreach (Question question in questionsToRemove)
			{
				m_questionsToIgnore.Add(question.Id);
				m_questions.Remove(m_questions.Single(q => q.Id == question.Id));
			}
			QuestionsChanged(this, EventArgs.Empty);
			AppendQuestions();
		}

		protected void ShowException(Exception ex)
		{
			ExceptionOcurred(this, new ExceptionEventArgs(ex));
		}
	}
}