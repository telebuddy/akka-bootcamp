﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail
{
    class TailCoordinatorActor : UntypedActor
    {
        #region Message Types
        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; private set; }
            public IActorRef ReporterActor { get; private set; }
        }

        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;
            }

            public object FilePath { get; private set; }
        }
        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;

                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10,
                TimeSpan.FromSeconds(30),
                x =>
                {
                    if (x is ArithmeticException) return Directive.Resume;

                    else if (x is NotSupportedException) return Directive.Stop;

                    else return Directive.Restart;
                });
        }
    }
}
 