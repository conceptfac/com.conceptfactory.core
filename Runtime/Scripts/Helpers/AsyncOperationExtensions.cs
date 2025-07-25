using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Concept.Helpers
{
    public static class AsyncOperationExtensions
    {
        // Método de extensão para transformar AsyncOperation em Task
        public static Task ToTask(this AsyncOperation asyncOperation)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Quando a operação for concluída, resolva a tarefa.
            asyncOperation.completed += (op) => tcs.SetResult(true);

            return tcs.Task;
        }

        // Método para aguardar a conclusão do carregamento da cena
        public static async Task WaitForSceneLoadAsync(AsyncOperation asyncLoad)
        {
            // Enquanto a cena não estiver carregada, aguarde
            while (!asyncLoad.isDone)
            {
                await Task.Yield(); // Aguarda até o próximo frame
            }

        }


        /// <summary>
        /// Call an action with delay or in End of Frame.
        /// </summary>
        /// <param name="action">Method to call</param>
        /// <param name="delay">Delay time in milliseconds, let empty to wait for End of Frame.</param>
        public async static void CallDelayedAction(Action action, int delay = -1)
        {
            if (delay < 0)
                await Task.Yield();
            else
            {
                int elapsed = 0;
                while (elapsed < delay)
                {
                    await Task.Yield();
                    elapsed += (int)(Time.unscaledDeltaTime * 1000);
                }
            }
            action?.Invoke();
        }


        [Obsolete("Task.Delay is multithread not works in WebGL")]
        public async static void CallDelayedActionOld(Action action, int delay = -1)
        {
            if (delay < 0)
                await Task.Yield();
            else
                await Task.Delay(delay);

            action?.Invoke();
        }

    }
}
