using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;
using YahurrLexer;

namespace YahurrFramework.Structs
{
	internal class PermissionClass
	{
		public string Name { get; private set; }

		public List<Permission> Permissions { get; private set; }

		public Dictionary<string, string> Properties { get; private set; }

		public PermissionGroup this[string index]
		{
			get
			{
				permissonGroups.TryGetValue(index, out PermissionGroup group);
				return group;
			}
		}

		Dictionary<string, PermissionGroup> permissonGroups;

		public PermissionClass(string moduleID)
		{
			Name = moduleID;
			Permissions = new List<Permission>();
			permissonGroups = new Dictionary<string, PermissionGroup>();
			Properties = new Dictionary<string, string>();
		}

		public void AddPermission(Permission permission)
		{
			Permissions.Add(permission);
		}

		public void AddPermissionGroup(PermissionGroup group)
		{
			permissonGroups.Add(group.Name, group);
		}

		public static PermissionClass Parse(Lexer<PermissionTokenType> lexer, string moduleID)
		{
			PermissionClass @class = new PermissionClass(moduleID);
			PermissionGroup premissionGroup = null;
			PermissionGroupType state = PermissionGroupType.Class;

			Token<PermissionTokenType> token = lexer.GetToken();
			while (token != null)
			{
				if (token.Type == PermissionTokenType.TargetType)
				{
					if (state == PermissionGroupType.Class)
						@class.AddPermission(ParsePermission(lexer));
					else if (premissionGroup != null)
						premissionGroup.AddPermission(ParsePermission(lexer));
				}
				else if (token.Type == PermissionTokenType.Bracket)
				{
					PermissionGroup group = ParseBracket(lexer, @class);
					state = group.Type;

					if (state == PermissionGroupType.Class)
					{
						@class.Properties = group.Properties;
					}
					else
					{
						premissionGroup = group;
						@class.AddPermissionGroup(group);
					}
				}
				else
				{
					lexer.NextToken();
				}

				token = lexer.GetToken();
			}

			return @class;
		}

		static Permission ParsePermission(Lexer<PermissionTokenType> lexer)
		{
			PermissionType type;
			PermissionTarget targetType;
			List<ulong> targets = new List<ulong>();

			Token<PermissionTokenType> token = lexer.GetToken();
			if (token.Type == PermissionTokenType.TargetType)
				type = Enum.Parse<PermissionType>(token.Value, true);
			else
				throw new Exception($"Wrong token type got {token.Type} expected TargetType");

			token = lexer.NextToken();
			if (token.Type == PermissionTokenType.Group)
				targetType = Enum.Parse<PermissionTarget>(token.Value, true);
			else
				throw new Exception($"Wrong token type got {token.Type} expected TargetGroup");

			token = lexer.NextToken();
			while (token?.Type == PermissionTokenType.Number)
			{
				targets.Add(ulong.Parse(token.Value));

				token = lexer.NextToken();
			}

			return new Permission(type, targetType, targets);
		}

		static PermissionGroup ParseBracket(Lexer<PermissionTokenType> lexer, PermissionClass @class)
		{
			PermissionGroupType type = PermissionGroupType.Method;
			string name = "";

			Token<PermissionTokenType> token = lexer.GetToken();
			if (token.Type == PermissionTokenType.Bracket && token.Value != "<")
				throw new Exception($"Wrong token got {token.Value} expected <");

			token = lexer.NextToken();
			if (token.Type == PermissionTokenType.Type)
			{
				type = Enum.Parse<PermissionGroupType>(token.Value, true);
				token = lexer.NextToken();
			}

			if (token.Type == PermissionTokenType.Colon)
				token = lexer.NextToken();

			if (token.Type == PermissionTokenType.Text && lexer.Peek(1)?.Type != PermissionTokenType.Colon)
				name = token.Value;
			else if (lexer.Peek(1)?.Type != PermissionTokenType.Colon)
				throw new Exception($"Wrong token type got {token.Type} expected Type or Text");

			Dictionary<string, string> properties = ParseProperties(lexer);

			token = lexer.GetToken();
			if (token.Type == PermissionTokenType.Bracket && token.Value == ">")
			{
				lexer.NextToken();
				return new PermissionGroup(@class, name, type, properties);
			}
			else
				throw new Exception($"Wrong token got {token.Value} expected >");
		}

		static Dictionary<string, string> ParseProperties(Lexer<PermissionTokenType> lexer)
		{
			Dictionary<string, string> properties = new Dictionary<string, string>();

			Token<PermissionTokenType> token = lexer.GetToken();
			while (token?.Type == PermissionTokenType.Text || token?.Type == PermissionTokenType.Operator)
			{
				if (token.Type == PermissionTokenType.Operator)
				{
					string key = lexer.Peek(-1)?.Value;
					string value = lexer.Peek(1).Value;

					properties.Add(key, value);
				}

				token = lexer.NextToken();
			}

			return properties;
		}
	}
}
