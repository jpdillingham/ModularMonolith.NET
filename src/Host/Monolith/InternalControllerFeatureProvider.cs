using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

/// <summary>
///     Allows the Host to discover Controllers marked 'internal'
/// </summary>
internal class InternalControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        var isCustomController = !typeInfo.IsAbstract
                    && typeof(ControllerBase).IsAssignableFrom(typeInfo)
                    && IsInternal(typeInfo);

        return isCustomController || base.IsController(typeInfo);

        bool IsInternal(TypeInfo t) =>
            !t.IsVisible
            && !t.IsPublic
            && t.IsNotPublic
            && !t.IsNested
            && !t.IsNestedPublic
            && !t.IsNestedFamily
            && !t.IsNestedPrivate
            && !t.IsNestedAssembly
            && !t.IsNestedFamORAssem
            && !t.IsNestedFamANDAssem;
    }
}
