using System;
namespace Library
{
    public class EmptyClass
    {
        public EmptyClass()
        {
            DoSomethingLikeInitialize();

            return;
        }

        #if XAMARIN_AUTH_INTERNAL
        public void DoSomethingLikeInitialize()
        {
            return;
        }
        #endif
    }
}
