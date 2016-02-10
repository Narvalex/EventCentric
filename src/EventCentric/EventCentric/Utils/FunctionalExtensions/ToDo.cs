//using System;
//namespace EventCentric
//{
//    public static class ChainedIfExtensions
//    {
//        //public static T If<T>(this T subject, Func<T, bool> predicate, Func<T, T> then) =>
//        //    subject.If(predicate.Invoke(subject), then);

//        public static T If<T>(this T subject, bool conditional, Func<T> then) =>
//            conditional ? then.Invoke() : subject;
//    }
//}