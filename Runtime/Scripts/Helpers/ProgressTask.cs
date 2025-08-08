using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Concept.Helpers
{


    /// <summary>
    /// Wrapper class that encapsulates a <see cref="Task{TResult}"/> allowing registration of a progress callback.
    /// </summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    public class ProgressTask<TResult>
    {
        // Internal task representing the asynchronous operation
        private readonly Task<TResult> _task;

        // Callback to report progress, receives a float value between 0 and 1
        private Action<float> _progressCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressTask{TResult}"/> class with the specified task.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        public ProgressTask(Task<TResult> task)
        {
            _task = task;
        }

        /// <summary>
        /// Registers a callback to receive progress updates.
        /// </summary>
        /// <param name="progressCallback">The action invoked when progress changes (value between 0 and 1).</param>
        /// <returns>The current instance for fluent method chaining.</returns>
        public ProgressTask<TResult> OnProgress(Action<float> progressCallback)
        {
            _progressCallback = progressCallback;
            return this;
        }

        /// <summary>
        /// Reports progress to the registered callback, if any.
        /// </summary>
        /// <param name="progress">The progress value between 0 and 1.</param>
        public void ReportProgress(float progress)
        {
            _progressCallback?.Invoke(progress);
        }

        /// <summary>
        /// Returns the awaiter of the internal task, enabling the instance to be awaited.
        /// </summary>
        /// <returns>The awaiter of the encapsulated task.</returns>
        public TaskAwaiter<TResult> GetAwaiter()
        {
            return _task.GetAwaiter();
        }
    }

}