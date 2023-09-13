using StyletIoC;

namespace ImageManager.Tools.Extension
{
    public static class ContainerEx
    {
        /// <summary>
        /// BuildUp扩展,在BuildUp后调用IInjectionAware的ParametersInjected方法。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="obj"></param>
        public static void BuildUpEx(this IContainer container, object obj)
        {
            container.BuildUp(obj);
            if (obj is IInjectionAware injectionAware)
            {
                injectionAware.ParametersInjected();
            }
        }
    }
}
