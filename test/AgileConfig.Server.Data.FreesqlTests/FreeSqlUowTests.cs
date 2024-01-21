using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Data.Freesql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Freesql.Tests
{
    [TestClass()]
    public class FreeSqlUowTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var configMap = new Dictionary<string, string>() {
                {"db:provider","sqlite" },
                {"db:conn","Data Source=agile_config.db" },
                {"db:env:test:provider","sqlite" },
                {"db:env:test:conn","Data Source=agile_config1.db" },
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(configMap);
            var configuration = configurationBuilder.Build();
            Global.Config = configuration;

            var fsql = FreeSQL.GetInstanceByEnv("");
            fsql.CodeFirst.SyncStructure<User_test>();
            fsql.CodeFirst.SyncStructure<Address_test>();
            fsql.Delete<User_test>(new User_test() { Id = 1 }).ExecuteAffrows();
            fsql.Delete<Address_test>(new Address_test() { Id = 1 }).ExecuteAffrows();
        }


        [TestMethod()]
        public async Task SaveChangesAsyncTest_success()
        {
            // arrange
            var fsql = FreeSQL.GetInstanceByEnv("");
            var user = new User_test()
            {
                Id = 1,
                Name = "abc"
            };
            var address = new Address_test()
            {
                Id = 1,
                Address = "Address"
            };
            // act
            using var uow = new FreeSqlUow(fsql);
         
            var userrepository = fsql.GetRepository<User_test>();
            userrepository.UnitOfWork = uow.GetFreesqlUnitOfWork();
            var addressrepository = fsql.GetRepository<Address_test>();
            addressrepository.UnitOfWork = uow.GetFreesqlUnitOfWork();

            uow.Begin();

            userrepository.Insert(user);
            user.Name = "test1";
            userrepository.Update(user);
            addressrepository.Insert(address);
            address.Address = "test1";
            addressrepository.Update(address);

            await uow.SaveChangesAsync();

            // assert
            var username = fsql.GetRepository<User_test>().Where(x => x.Id == 1).ToOne().Name;
            var add = fsql.GetRepository<Address_test>().Where(x => x.Id == 1).ToOne().Address;
            Assert.AreEqual("test1", username);
            Assert.AreEqual("test1", add);
        }

        [TestMethod()]
        public async Task SaveChangesAsyncTest_rollback()
        {
            // arrange
            var fsql = FreeSQL.GetInstanceByEnv("");
            var user = new User_test()
            {
                Id = 2,
                Name = "abc"
            };
            var address = new Address_test()
            {
                Id = 2,
                Address = "Address"
            };

            // act
            using var uow = new FreeSqlUow(fsql);
            var userrepository = fsql.GetRepository<User_test>();
            userrepository.UnitOfWork = uow.GetFreesqlUnitOfWork();
            var addressrepository = fsql.GetRepository<Address_test>();
            addressrepository.UnitOfWork = uow.GetFreesqlUnitOfWork();

            uow.Begin();
            try
            {
                userrepository.Insert(user);
                user.Name = "test1";
                userrepository.Update(user);
                throw new Exception("test");
                addressrepository.Insert(address);
                address.Address = "test1";
                addressrepository.Update(address);

                await uow.SaveChangesAsync();
            }
            catch (Exception exc)
            {
                Assert.IsNotNull(exc);
            }


            // assert
            var _user = fsql.GetRepository<User_test>().Where(x => x.Id == 2).ToOne();
            var _address = fsql.GetRepository<Address_test>().Where(x => x.Id == 2).ToOne();

            Assert.IsNull(_user);
            Assert.IsNull(_address);
        }
    }

    public class User_test
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Address_test
    {
        public int Id { get; set; }
        public string Address { get; set; }
    }
}