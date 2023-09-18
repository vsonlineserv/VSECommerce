using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Infrastructure
{
    public class Guard
    {
        private const string AgainstMessage = "Assertion evaluation failed with 'false'.";
        private const string ImplementsMessage = "Type '{0}' must implement type '{1}'.";
        private const string InheritsFromMessage = "Type '{0}' must inherit from type '{1}'.";
        private const string IsTypeOfMessage = "Type '{0}' must be of type '{1}'.";
        private const string IsEqualMessage = "Compared objects must be equal.";
        private const string IsPositiveMessage = "Argument '{0}' must be a positive value. Value: '{1}'.";
        private const string IsTrueMessage = "True expected for '{0}' but the condition was False.";
        private const string NotNegativeMessage = "Argument '{0}' cannot be a negative value. Value: '{1}'.";

        private Guard()
        {
        }

        /// <summary>
        /// Throws proper exception if the class reference is null.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value">Class reference to check.</param>
        /// <exception cref="InvalidOperationException">If class reference is null.</exception>
        [DebuggerStepThrough]
        public static void NotNull<TValue>(Func<TValue> value)
        {
            if (value() == null)
                throw new InvalidOperationException("'{value}' cannot be null.".FormatWith(new { value }));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotNull(object arg, string argName)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        [DebuggerStepThrough]
        public static void ArgumentNotNull<T>(Func<T> arg)
        {
            if (arg() == null)
                throw new ArgumentNullException(GetParamName(arg));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2>(Func<T1> arg1, Func<T2> arg2)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2, T3>(Func<T1> arg1, Func<T2> arg2, Func<T3> arg3)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));

            if (arg3() == null)
                throw new ArgumentNullException(GetParamName(arg3));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2, T3, T4>(Func<T1> arg1, Func<T2> arg2, Func<T3> arg3, Func<T4> arg4)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));

            if (arg3() == null)
                throw new ArgumentNullException(GetParamName(arg3));

            if (arg4() == null)
                throw new ArgumentNullException(GetParamName(arg4));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2, T3, T4, T5>(Func<T1> arg1, Func<T2> arg2, Func<T3> arg3, Func<T4> arg4, Func<T5> arg5)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));

            if (arg3() == null)
                throw new ArgumentNullException(GetParamName(arg3));

            if (arg4() == null)
                throw new ArgumentNullException(GetParamName(arg4));

            if (arg5() == null)
                throw new ArgumentNullException(GetParamName(arg5));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Func<string> arg)
        {
            if (string.IsNullOrEmpty(arg()))
                throw new ArgumentNullException(arg.ToString());
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Func<Guid> arg)
        {
            if (arg() == Guid.Empty)
            {
                string argName = GetParamName(arg);
                throw new ArgumentNullException(argName, "Argument '{argName}' cannot be an empty guid.".FormatWith(new { argName }));

            }
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Func<IEnumerable> arg)
        {
            if (!arg().HasItems())
            {
                string argName = GetParamName(arg);
                throw new ArgumentNullException(argName, "List cannot be null and must have at least one item.");
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(string arg, string argName)
        {
            if (string.IsNullOrEmpty(arg))
                throw new ArgumentNullException(argName, "String parameter '{0}' cannot be null or all whitespace.".FormatWith(new { argName }));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(IEnumerable arg, string argName)
        {
            if (!arg.HasItems())
                throw new ArgumentNullException(argName, "List cannot be null and must have at least one item.");
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Guid arg, string argName)
        {
            if (arg == Guid.Empty)
                throw new ArgumentNullException(argName, "String parameter '{argName}' cannot be null or all whitespace.".FormatWith(new { argName }));
            throw new ArgumentNullException(argName, "Argument '{0}' cannot be an empty guid.");
        }

        [DebuggerStepThrough]
        public static void ArgumentInRange<T>(T arg, T min, T max, string argName) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(min) < 0 || arg.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(argName, "The argument '{argName}' must be between'.".FormatWith(new { argName }));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotOutOfLength(string arg, int maxLength, string argName)
        {
            if (arg.Trim().Length > maxLength)
            {
                throw new ArgumentOutOfRangeException(argName, "Argument '{0}' cannot be more than characters long.".FormatWith(new { argName }));
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentNotNegative<T>(T arg, string argName, string message = NotNegativeMessage) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) < 0)
                throw new ArgumentOutOfRangeException(argName, message.FormatWith(new { argName, arg }));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotZero<T>(T arg, string argName) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) == 0)
                throw new ArgumentOutOfRangeException(argName, "Argument '{argName}' must be greater or less than zero. Value: '{arg}'.".FormatWith(new { argName, arg }));
        }

        [DebuggerStepThrough]
        public static void Against<TException>(bool assertion, string message = AgainstMessage) where TException : Exception
        {
            if (assertion)
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }

        [DebuggerStepThrough]
        public static void Against<TException>(Func<bool> assertion, string message = AgainstMessage) where TException : Exception
        {
            //Execute the lambda and if it evaluates to true then throw the exception.
            if (assertion())
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }





        [DebuggerStepThrough]
        public static void IsTypeOf<TType>(object instance, string message)
        {
            if (!(instance is TType))
                throw new InvalidOperationException(message);
        }

        [DebuggerStepThrough]
        public static void IsEqual<TException>(object compare, object instance, string message = IsEqualMessage) where TException : Exception
        {
            if (!compare.Equals(instance))
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }



        [DebuggerStepThrough]
        private static string GetParamName<T>(Expression<Func<T>> expression)
        {
            string name = string.Empty;
            MemberExpression body = expression.Body as MemberExpression;

            if (body != null)
            {
                name = body.Member.Name;
            }

            return name;
        }

        [DebuggerStepThrough]
        private static string GetParamName<T>(Func<T> expression)
        {
            return expression.Method.Name;
        }

    }
}
