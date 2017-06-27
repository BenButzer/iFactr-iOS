using System;
using System.Linq;
using System.Reflection;

using iFactr.UI;

namespace iFactr.Touch
{
	internal static class EventRaiser
	{
		internal static bool RaiseEvent(this IPairable obj, string eventName, EventArgs args)
		{
			var type = obj.GetType();
			var evt = type.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);
			if (evt != null)
			{
				var attribute = evt.GetCustomAttributes<EventDelegateAttribute>().FirstOrDefault();
				if (attribute != null)
				{
					eventName = attribute.DelegateName;
				}

				FieldInfo info = null;
				do
				{
					info = type.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
					type = type.BaseType;
				}
				while (info == null && type != null);

				if (info != null)
				{
					var del = info.GetValue(obj) as MulticastDelegate;
					if (del != null)
					{
						var invocationList = del.GetInvocationList();
						foreach (var method in invocationList)
						{
							method.Method.Invoke(method.Target, new object[] { obj.Pair ?? obj, EventArgs.Empty });
						}
						return true;
					}
				}
			}

			return false;
		}
	}
}

