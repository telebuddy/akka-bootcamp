﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail
{
    class TailActor : UntypedActor
    {
        #region Message Types
        public class FileWrite
        {
            public FileWrite(string filename)
            {
                FileName = filename;
            }

            public object FileName { get; private set; }
        }

        public class FileError
        {
            public FileError(string filename, string reason)
            {
                FileName = filename;
                Reason = reason;
            }

            public object FileName { get; private set; }
            public object Reason { get; private set; }
        }

        public class InitialRead
        {
            public InitialRead(string filename, string text)
            {
                FileName = filename;
                Text = text;
            }

            public object FileName { get; private set; }
            public object Text { get; private set; }
        }
        #endregion

        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _observer;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }
            else if (message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell("Tail error: {fe.Reason}");
                   
            }
            else if (message is InitialRead)
            {
                var ir = message as InitialRead;
                _reporterActor.Tell(ir.Text);
            }
        }
    }
}
