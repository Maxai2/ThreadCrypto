using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadCrypto
{
    class EventArgs<TResult> : EventArgs
    {
        public EventArgs(TResult result)
        {
            this.Result = result;
        }
        public TResult Result { get; }
    }

    // // /// <summary>
    /// Примитивная реализация класса <see cref="System.Threading.Tasks.Task"/>.
    /// Обертка над классом <see cref="System.Threading.Thread"/>, позволяющая исполнять методы, возвращающие значение.
    /// </summary>
    /// <typeparam name="TParam">Тип параметра метода</typeparam>
    /// <typeparam name="TResult">Тип возвращаемого значения метода</typeparam>
    class ThreadItem<TParam, TResult>
    {
        readonly Thread thread;
        readonly Func<TParam, TResult> func;
        TResult result;

        /// <summary>
        /// Проверяет, закончил ли поток свою работу.
        /// </summary>
        public bool Ended
        {
            get
            {
                return !this.thread.IsAlive;
            }
        }

        /// <summary>
        /// Возвращает результат работы потока. Если поток не завершил свою работу - ожидает завершения.
        /// </summary>
        public TResult Result
        {
            get
            {
                if (!this.Ended)
                {
                    this.thread.Join();
                }
                return this.result;
            }
        }

        /// <summary>
        /// Конструктор, создающий объект <see cref="ThreadItem{TParam, TResult}"/>.
        /// </summary>
        /// <param name="func">Метод, который будет исполняться в потоке.</param>
        /// <param name="isBackground">Является ли поток фоновым</param>
        public ThreadItem(Func<TParam, TResult> func, bool isBackground = false)
        {
            this.func = func;

            this.thread = new Thread((param) =>
            {
                this.result = this.func((TParam)param);
                Finished?.Invoke(this, new EventArgs<TResult>(this.result));
            });
            this.thread.IsBackground = isBackground;
        }

        /// <summary>
        /// Запускает поток на выполнение.
        /// </summary>
        /// <param name="param">Аргумент, передаваемый в исполняемый метод.</param>
        public void Start(TParam param = default)
        {
            this.thread.Start(param);
        }

        /// <summary>
        /// Событие, оповещяющее о завершении работы потока и получении результата.
        /// </summary>
        public event EventHandler<EventArgs<TResult>> Finished;

        /// <summary>
        /// Отменяет исполнение потока.
        /// </summary>
        public void Cancel()
        {
            this.thread.Interrupt();
        }
    }
}
