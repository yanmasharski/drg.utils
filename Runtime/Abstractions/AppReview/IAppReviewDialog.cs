using System;
using System.Threading.Tasks;

namespace DRG.Utils
{
	public interface IAppReviewDialog
	{
		void Show(Action<bool> onComplete = null);
		Task<bool> ShowAsync();
	}
}

