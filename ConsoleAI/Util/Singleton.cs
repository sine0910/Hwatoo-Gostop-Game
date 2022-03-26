using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    public abstract class CSingleton<T> where T : class, new()
    {
        static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }

                return instance;
            }
        }
    }
}
