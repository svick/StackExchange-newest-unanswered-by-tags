using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		LinkedList<Question> questions = new LinkedList<Question>();
		public virtual IEnumerable<Question> Questions
		{
			get
			{
				return questions;
			}
		}

		public int QuestionsToLoad
		{
			get;
			set;
		}

		StackyClient client;
		Site site;
		public virtual Site Site
		{
			get
			{
				return site;
			}
			set
			{
				setSite(value);
				reset();
			}
		}

		void setSite(Site value)
		{
			site = value;
			client = new StackyClient("1.0", null, site, new UrlClient(), new JsonProtocol());
		}

		HashSet<string> tags;
		public virtual IEnumerable<string> Tags
		{
			get
			{
				return tags;
			}
			set
			{
				setTags(value);
				reset();
			}
		}

		void setTags(IEnumerable<string> value)
		{
			tags = new HashSet<string>(value);
		}

		public event EventHandler QuestionsChanged = delegate { };
		public event EventHandler<ExceptionEventArgs> ExceptionOcurred = delegate { };

		public QuestionsManager(Site site, IEnumerable<string> tags)
		{
			QuestionsToLoad = 20;
			setSite(site);
			setTags(tags);
		}

		//what if we reach end?
		IEnumerable<Question> loadQuestions()
		{
			int page = 1;
			while (true)
			{
				IEnumerable<Question> questions = client.GetQuestions(
					sortBy: QuestionSort.UnansweredCreation,
					pageSize: PageSize,
					page: page++);

				if (Tags != null && Tags.Any())
					questions = questions.Where(q => q.Tags.Any(tag => tags.Contains(tag)));

				foreach (Question question in questions)
					yield return question;
			}
		}

		IEnumerator<Question> incoming = null;

		//expects that loadQuestions() returns "infinite" sequence
		protected void processQuestions()
		{
			incoming = loadQuestions().GetEnumerator();
			incoming.MoveNext();

			if (questions.Any())
			{
				var current = questions.First;
				while (current != null)
				{
					if (incoming.Current.Id > current.Value.Id)
					{
						questions.AddBefore(current, incoming.Current);
						incoming.MoveNext();
					}
					else if (incoming.Current.Id < current.Value.Id)
					{
						var toDelete = current;
						current = current.Next;
						questions.Remove(toDelete);
					}
					else
					{
						current.Value = incoming.Current;
						current = current.Next;
						incoming.MoveNext();
					}
				}
			}

			appendQuestions();
		}

		protected virtual void appendQuestions()
		{
			while (appendQuestion())
				;
		}

		protected bool appendQuestion()
		{
			if (questions.Count < QuestionsToLoad)
			{
				questions.AddLast(incoming.Current);
				incoming.MoveNext();
			}

			QuestionsChanged(this, EventArgs.Empty);

			return questions.Count < QuestionsToLoad;
		}

		void reset()
		{
			questions.Clear();

			if (client != null)
			{
				incoming = loadQuestions().GetEnumerator();
				incoming.MoveNext();
				appendQuestions();
			}
		}

		public virtual void Remove(Question question)
		{
			questions.Remove(question);
			appendQuestions();
		}

		public virtual void Remove(IEnumerable<Question> questionsToRemove)
		{
			foreach (Question question in questionsToRemove)
				questions.Remove(question);
			appendQuestions();
		}

		protected void showException(Exception ex)
		{
			ExceptionOcurred(this, new ExceptionEventArgs(ex));
		}
	}
}