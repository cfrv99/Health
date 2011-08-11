using System.Collections.Generic;
using System.Web.Mvc;
using Health.API;
using Health.API.Entities;
using Health.API.Repository;
using Health.API.Services;
using Health.API.Validators;
using Health.Core;
using Health.Core.Services;
using Health.Data.Entities;
using Health.Data.Repository.Fake;
using Health.Data.Validators;
using Health.Site.App_Start;
using Health.Site.Areas.Account.Models.Forms;
using Health.Site.Attributes;
using Health.Site.DI;
using Health.Site.Filters;
using Health.Site.Models.Binders;
using Health.Site.Repository;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Mvc;
using Ninject.Web.Mvc.FilterBindingSyntax;
using NinjectAdapter;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof (NinjectMVC3), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectMVC3), "Stop")]

namespace Health.Site.App_Start
{
    public static class NinjectMVC3
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        public static IKernel Kernel { get; private set; }

        /// <summary>
        /// ������ ����������.
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof (HttpApplicationInitializationModule));
            Bootstrapper.Initialize(CreateKernel);
            ModelToBinder();
        }

        /// <summary>
        /// ����������� ������������� �������� ��� �������
        /// </summary>
        public static void ModelToBinder()
        {
            ModelBinders.Binders.Add(typeof (InterviewFormModel), new ParametersFormBinder(Kernel.Get<IDIKernel>()));
        }

        /// <summary>
        /// ��������� ����������.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        /// �������� ����.
        /// </summary>
        /// <returns>��������� ����.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            var locator = new NinjectServiceLocator(kernel);
            ServiceLocator.SetLocatorProvider(() => locator);
            RegisterServices(kernel);
            Kernel = kernel;
            return kernel;
        }

        /// <summary>
        /// ����������� �����������.
        /// </summary>
        /// <param name="kernel">����.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // �������� 
            kernel.Bind<IRole>().To<Role>();
            kernel.Bind<IUser>().To<User>();
            kernel.Bind<IDefaultRoles>().To<DefaultRoles>();
            kernel.Bind<IUserCredential>().To<UserCredential>();
            kernel.Bind<IParameter>().To<Parameter>();
            // �����������
            kernel.Bind<IRoleRepository>().To<RolesFakeRepository>().InSingletonScope();
            kernel.Bind<IUserRepository>().To<UsersFakeRepository>().InSingletonScope();
            kernel.Bind<IActualCredentialRepository>().To<SessionRepository>();
            kernel.Bind<IPermanentCredentialRepository>().To<CookieRepository>();
            kernel.Bind<ICandidateRepository>().To<CandidatesFakeRepository>().InSingletonScope();
            // �������
            kernel.Bind<ICoreKernel>().To<CoreKernel>().InSingletonScope();
            kernel.Bind<IAuthorizationService>().To<AuthorizationService>();
            kernel.Bind<IRegistrationService>().To<RegistrationService<Candidate>>();
            // �������
            kernel.Bind<IValidatorFactory>().To<ValidatorFactory>();
            // ������� ��� ���������
            kernel.BindFilter<AuthFilter>(FilterScope.Controller, 0).WhenActionMethodHas<Auth>().
                WithConstructorArgumentFromActionAttribute<Auth>("allow_roles", att => att.AllowRoles).
                WithConstructorArgumentFromActionAttribute<Auth>("deny_roles", att => att.DenyRoles);
            // ������
            kernel.Bind<IDIKernel>().To<DIKernel>();
            kernel.Bind<IEnumerable<IParameter>>().To<List<Parameter>>();
            kernel.Bind<ILogger>().To<Logger>().WithConstructorArgument("class_name", c => c.Request.Service.Name);
        }
    }
}