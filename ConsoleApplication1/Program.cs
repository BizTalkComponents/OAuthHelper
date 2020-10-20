using BizTalkComponents.CustomComponents.OAuthTokenHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var t= TokenHelper.GetToken("https://login.microsoftonline.com/1f2f9fd0-1fb7-43a7-bd21-67affa7442ca/oauth2/token"
                , "2abd1d2a-9ea6-422c-982d-6bbe59b8a74f"
                , "wks_ir.w-b_Y~Ei3q4XhM9KmPF10EyyLbn"
                , "resource=api://ace7b17f-0573-4713-9694-b7e546a213b2"
                //+ "&audience=api://ace7b17f-0573-4713-9694-b7e546a213b2"
                //+ "&scope=api://ace7b17f-0573-4713-9694-b7e546a213b2/Scope"
                );            
        }
    }
}
