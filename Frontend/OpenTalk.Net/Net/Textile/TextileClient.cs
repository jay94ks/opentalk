﻿using OpenTalk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace OpenTalk.Net.Textile
{
    public enum TextileState
    {
        Handshaking,
        Ready,
        Refused,
        Unreachable,
        Disconnected
    }

    public class TextileClient
    {
        public struct Message
        {
            public string Label { get; set; }
            public string Data { get; set; }
        }

        private TcpClient m_Client;
        private TextileState m_State;

        /// <summary>
        /// 수신 버퍼는, 2개가 한쌍입니다.
        /// 즉, 실제 수신된 메시지는 m_Receives.Count / 2 개 입니다.
        /// 
        /// 첫번째 문자열은 레이블이고,
        /// 두번째 문자열은 데이터 본문입니다.
        /// </summary>
        private Queue<string> m_Receives;
        private Dictionary<string, Action<TextileClient, Message>> m_Handlers;

        /*
         * 암호화 키의 패턴이 발각되는 것을 방지하기 위해서,
         * 철저한 Offset관리를 이용, Cipher가 반드시 
         * 좌우 회전식으로 동작하도록 구성합니다.
         */
        private byte[] m_DecryptKey, m_EncryptKey;
        private int m_DecryptOffset, m_EncryptOffset;

        private MemoryStream m_DecodingBuffer = new MemoryStream();
        private byte[] m_ReceiveBuffer = new byte[256];
        private int m_ReceiveStage;

        /// <summary>
        /// 이 TextileClient의 엔코딩은 UTF8로 고정되어 있습니다.
        /// </summary>
        private static readonly Encoding Encoding = Encoding.UTF8;
        private static readonly int KeyLength = 8192;
        private static Random KeyGenerator = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// 이미 접속 처리가 완료된 Tcp 클라이언트 객체로
        /// TextileClient 객체를 초기화합니다.
        /// </summary>
        /// <param name="Client"></param>
        public TextileClient(TcpClient Client)
        {
            m_Client = Client;

            m_Receives = new Queue<string>();
            m_Handlers = new Dictionary<string, Action<TextileClient, Message>>();

            ArmEncryptionKey();
            m_DecryptOffset = m_EncryptOffset = 0;

            m_Client.ReadReady += OnReadReady;
            m_Client.Closed += OnClosed;

            m_State = TextileState.Handshaking;
            OnReady(m_Client, null);
        }

        /// <summary>
        /// 호스트 어드레스와 포트 번호로
        /// TextileClient 객체를 초기화합니다.
        /// </summary>
        /// <param name="Client"></param>
        public TextileClient(IPAddress address, int port)
            : this(new TcpClient())
        {
            m_Client.Ready += OnReady;
            m_Client.Refused += OnRefused;
            m_Client.Unreachable += OnUnreachable;

            m_State = TextileState.Handshaking;
            m_Client.Connect(address, port);
        }

        /// <summary>
        /// 호스트 이름과 포트 번호로
        /// TextileClient 객체를 초기화합니다.
        /// </summary>
        /// <param name="Client"></param>
        public TextileClient(string host, int port)
            : this(new TcpClient())
        {
            m_Client.Ready += OnReady;
            m_Client.Refused += OnRefused;
            m_Client.Unreachable += OnUnreachable;

            m_State = TextileState.Handshaking;
            m_Client.Connect(host, port);
        }

        /// <summary>
        /// 이 소켓의 상태를 조사합니다.
        /// </summary>
        public TextileState State => this.Locked(() => m_State);

        /// <summary>
        /// 이 소켓의 상태가 변경되면 실행되는 이벤트입니다.
        /// (같은 상태가 반복적으로 실행될 수 있습니다)
        /// </summary>
        public event Action<TextileClient, TextileState> StateChanged;

        /// <summary>
        /// 이 소켓에 수신된 메시지가 있으면 실행되는 이벤트입니다.
        /// </summary>
        public event Action<TextileClient, int> MessageReady;

        /// <summary>
        /// 연결이 준비되면 실행되는 콜백입니다.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnReady(TcpClient socket, object arg2)
        {
            OnStateChanged(TextileState.Handshaking);

            SendEncryptionKey();
            m_Client.ReadBuffer.WaitBytes(sizeof(ushort), OnHandshakeFirstBytes);
        }

        /// <summary>
        /// 암호화 키를 생성합니다.
        /// 이 소켓이 최초 초기화 될 때 단 한번만 생성합니다.
        /// </summary>
        private void ArmEncryptionKey()
        {
            m_EncryptKey = new byte[KeyLength];
            KeyGenerator.NextBytes(m_EncryptKey);
        }

        /// <summary>
        /// 암호화 키를 송신합니다.
        /// </summary>
        private void SendEncryptionKey()
        {
            byte[] KeyLengthBytes = BitConverter.GetBytes((ushort)m_EncryptKey.Length);
            m_Client.Write(KeyLengthBytes, 0, KeyLengthBytes.Length);
            m_Client.Write(m_EncryptKey, 0, m_EncryptKey.Length);
        }

        /// <summary>
        /// 원격 호스트를 찾을 수 없을 때 실행되는 콜백입니다.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnUnreachable(TcpClient socket, object arg2) => OnStateChanged(TextileState.Unreachable);

        /// <summary>
        /// 접속이 거부되면 실행되는 콜백입니다.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnRefused(TcpClient socket, object arg2) => OnStateChanged(TextileState.Refused);

        /// <summary>
        /// 수신 준비가 되면 실행되는 콜백입니다.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="arg2"></param>
        private void OnReadReady(IO.IReadInterface socket, int arg2)
        {
            BinaryBuffer buffer = socket.ReadBuffer;
            int PeekedSize = 0;

            // 준비되지 않은 상태에서는 수신되는 바이트들을
            // 이 콜백이 아닌, BufferWait 콜백들로 처리합니다.

            if (State != TextileState.Ready)
                return;

            while (!buffer.IsEmpty)
            {
                if (m_ReceiveStage <= 0)
                {
                    PeekedSize = buffer.Peek(
                        m_ReceiveBuffer, 0, sizeof(int));

                    if (PeekedSize >= sizeof(int))
                    {
                        DecryptBytes(m_ReceiveBuffer, 0, PeekedSize);

                        buffer.Consume(PeekedSize);
                        m_ReceiveStage = BitConverter.ToInt32(m_ReceiveBuffer, 0);
                        continue;
                    }

                    break;
                }
                else
                {
                    PeekedSize = Math.Min(m_ReceiveStage, m_ReceiveBuffer.Length);
                    PeekedSize = buffer.Peek(m_ReceiveBuffer, 0, PeekedSize);

                    if (PeekedSize > 0)
                    {
                        DecryptBytes(m_ReceiveBuffer, 0, PeekedSize);
                        buffer.Consume(PeekedSize);

                        m_DecodingBuffer.Write(m_ReceiveBuffer, 0, PeekedSize);
                        m_ReceiveStage -= PeekedSize;
                    }

                    // 수신 완료.
                    if (m_ReceiveStage <= 0)
                    {
                        lock (m_Receives)
                        {
                            m_Receives.Enqueue(Encoding.GetString(m_DecodingBuffer.ToArray()));
                        }

                        ResetDecodingBuffer();
                    }
                }
            }

            RaiseReceiveEventIfReady();
        }

        /// <summary>
        /// 수신 큐에 수신된 메시지열들이 완성된게 있다면,
        /// 이벤트를 발생시킵니다.
        /// </summary>
        private void RaiseReceiveEventIfReady()
        {
            lock (m_Receives)
            {
                while (m_Receives.Count / 2 > 0)
                {
                    string Label = m_Receives.Peek();
                    lock (m_Handlers)
                    {
                        if (m_Handlers.ContainsKey(Label))
                        {
                            Message message = new Message();

                            message.Label = m_Receives.Dequeue();
                            message.Data = m_Receives.Dequeue();

                            m_Handlers[Label]?.Invoke(this, message);
                            Label = null;
                        }
                    }

                    if (Label != null)
                        MessageReady?.Invoke(this, m_Receives.Count / 2);
                }
            }
        }

        /// <summary>
        /// 디코딩 버퍼를 리셋합니다.
        /// </summary>
        private void ResetDecodingBuffer()
        {
            byte[] Temp = m_DecodingBuffer.GetBuffer();

            Array.Resize(ref Temp, 0);
            m_DecodingBuffer.Position = 0;
            m_DecodingBuffer.SetLength(0);
        }

        /// <summary>
        /// 주어진 버퍼를 복호화합니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        private void DecryptBytes(byte[] buffer, int index, int count)
        {
            int Temp = 0;
            
            while (count > 0)
            {
                Temp = m_DecryptOffset;

                m_DecryptOffset = (m_DecryptOffset + 1) % m_DecryptKey.Length;
                buffer[index] = (byte)(buffer[index] ^ m_DecryptKey[Temp]);

                index++;
                count--;
            }
        }

        /// <summary>
        /// 주어진 버퍼를 암호화합니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        private void EncryptBytes(byte[] buffer, int index, int count)
        {
            int Temp = 0;

            while (count > 0)
            {
                Temp = m_EncryptOffset;

                m_EncryptOffset = (m_EncryptOffset + 1) % m_EncryptKey.Length;
                buffer[index] = (byte)(buffer[index] ^ m_EncryptKey[Temp]);

                index++;
                count--;
            }
        }

        /// <summary>
        /// 연결이 끊어지면 실행되는 이벤트입니다.
        /// </summary>
        /// <param name="obj"></param>
        private void OnClosed(TcpClient obj)
        {
            OnStateChanged(TextileState.Disconnected);
            m_Receives.Clear();
        }

        /// <summary>
        /// 상태가 변화하면 실행되는 메서드입니다.
        /// </summary>
        /// <param name="State"></param>
        protected virtual void OnStateChanged(TextileState State)
        {
            lock (this)
            {
                m_State = State;
            }

            StateChanged?.Invoke(this, State);
        }

        /// <summary>
        /// 핸드쉐이크 첫 바이트들이 수신되면 실행되는 콜백입니다.
        /// (암호 키의 길이가 수신됩니다)
        /// </summary>
        /// <param name="obj"></param>
        private void OnHandshakeFirstBytes(BinaryBuffer obj)
        {
            byte[] KeyLengthBytes = new byte[sizeof(ushort)];
            int Size = obj.Peek(KeyLengthBytes, 0, KeyLengthBytes.Length);

            if (Size >= KeyLengthBytes.Length)
            {
                obj.Consume(Size);

                m_DecryptKey = new byte[BitConverter.ToUInt16(KeyLengthBytes, 0)];
                obj.WaitBytes(m_DecryptKey.Length, OnHandshakeSecondBytes);
            }
        }

        /// <summary>
        /// 핸드쉐이크 두번째 바이트열이 수신되면 실행되는 콜백입니다.
        /// (암호 키 본문이 수신됩니다)
        /// </summary>
        /// <param name="obj"></param>
        private void OnHandshakeSecondBytes(BinaryBuffer obj)
        {
            int Size = obj.Peek(m_DecryptKey, 0, m_DecryptKey.Length);
            if (Size >= m_DecryptKey.Length)
            {
                obj.Consume(m_DecryptKey.Length);

                lock (this)
                    OnStateChanged(TextileState.Ready);

                OnReadReady(m_Client, obj.Size);
            }
        }
    }
}
