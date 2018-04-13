using System;
using VDS.Helpers.Exception;
using VDS.Mapping.Conventions;
using VDS.Mapping.Converters;
using VDS.Mapping.Runtime;

namespace VDS.Mapping.Mappers.MemberMapper
{
    internal class DefaultMemberMapper : MemberMapper
    {
        private readonly MappingMember _sourceMember;

        public DefaultMemberMapper(MappingContainer container, MemberMapOptions options, MappingMember targetMember, MappingMember sourceMember,
            ValueConverter converter)
            : base(container, options, targetMember, converter)
        {
            Throw.IfArgumentNull(sourceMember, nameof(sourceMember));
            _sourceMember = sourceMember;
        }

        public override Type SourceType => _sourceMember.MemberType;

        protected override void EmitSource(CompilationContext context)
        {
            ((IMemberBuilder)_sourceMember).EmitGetter(context);
        }
    }
}
