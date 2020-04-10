using System;
using System.Collections.Generic;

namespace OpenTalk
{
    public abstract partial class Application
    {
        public class NamedWorkers
        {
            private Dictionary<string, Worker> m_Workers;

            /// <summary>
            /// 이름 있는 작업자에 대한 관리를 수행하는 객체를 초기화합니다.
            /// </summary>
            internal NamedWorkers()
            {
                m_Workers = new Dictionary<string, Worker>();
            }

            /// <summary>
            /// 입력 문자열을 Dictionary 내에서 유일하게 존재하는 이름으로 변환합니다.
            /// </summary>
            /// <param name="inString"></param>
            /// <returns></returns>
            private string MakeUnique(string inString)
                => inString != null ? inString.ToLower() : "";

            /// <summary>
            /// 지정된 이름을 가진 작업자가 존재하는지 확인합니다.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public bool Has(string name)
            {
                lock (m_Workers)
                    return m_Workers.ContainsKey(name);
            }

            /// <summary>
            /// 등록된 작업자의 이름들을 획득합니다.
            /// </summary>
            public string[] Names {
                get {
                    lock (m_Workers)
                        return (new List<string>(m_Workers.Keys)).ToArray();
                }
            }

            /// <summary>
            /// 지정된 이름의 작업자를 획득합니다.
            /// 설정된 작업자가 없는 경우, KeyNotFoundException이 발생합니다.
            /// </summary>
            /// <param name="Name"></param>
            /// <returns></returns>
            private Worker Get(string Name)
            {
                lock (m_Workers)
                    return m_Workers[MakeUnique(Name)];
            }

            /// <summary>
            /// 지정된 이름으로 작업자를 설정합니다.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="worker"></param>
            /// <returns></returns>
            public bool Set(string name, Worker worker)
            {
                string UniqueName = MakeUnique(name);

                lock (m_Workers)
                {
                    if (worker != null)
                        m_Workers[UniqueName] = worker;

                    else if (m_Workers.ContainsKey(UniqueName))
                        return m_Workers.Remove(UniqueName);
                }

                return true;
            }

            /// <summary>
            /// 지정된 이름으로 작업자를 획득하거나 설정합니다.
            /// null을 지정할 경우엔, 해당 이름으로 등록된 작업자를 제거합니다.
            /// 
            /// 작업자를 획득하려는 경우에 지정된 이름으로 설정된 작업자가 없는 경우, 
            /// KeyNotFoundException이 발생합니다.
            /// </summary>
            /// <param name="Name"></param>
            /// <returns></returns>
            public Worker this[string Name] {
                get => Get(Name);
                set => Set(Name, value);
            }
        }

        
    }
}
