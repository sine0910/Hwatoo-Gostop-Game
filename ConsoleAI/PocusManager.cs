using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIProject
{
    class PocusManager
    {
        public delegate void callback();

        public PocusManager()
        {
        }

        public async Task<bool> delay_out_time(callback cb)
        {
            await Task.Delay(15000);

            cb();

            return true;
        }

    }
}
