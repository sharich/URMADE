[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(URMade.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(URMade.App_Start.NinjectWebCommon), "Stop")]

namespace URMade.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<URMade.Models.ApplicationDbContext>().ToSelf().InRequestScope();
            kernel.Bind<SecurityHelperRequestScopeCache>().ToSelf().InRequestScope();
            kernel.Bind<IUserStore<ApplicationUser>>()
                .To<UserStore<ApplicationUser>>()
                .WithConstructorArgument("context", context => kernel.Get<ApplicationDbContext>());

            RegisterRepo<UserRepository>(kernel);
            RegisterRepo<UserGroupRepository>(kernel);
            RegisterRepo<SelectOptionRepository>(kernel);
            RegisterRepo<ArtistRepository>(kernel);
			RegisterRepo<SongRepository>(kernel);
			RegisterRepo<VideoRepository>(kernel);
			RegisterRepo<ContestRepository>(kernel);
        }

        private static void RegisterRepo<TRepo>(IKernel kernel)
        {
            kernel.Bind<TRepo>().ToSelf()
                .WithConstructorArgument("context", context => kernel.Get<URMade.Models.ApplicationDbContext>());
        }        
    }
}
