﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uwl.Common.AutoMapper;
using Uwl.Common.LambdaTree;
using Uwl.Data.Model.Assist;
using Uwl.Data.Model.BaseModel;
using Uwl.Data.Model.Enum;
using Uwl.Data.Model.VO.Personal;
using Uwl.Domain.IRepositories;
using Uwl.Domain.RoleInterface;
using Uwl.Domain.UserInterface;
using Uwl.Extends.EncryPtion;
using Uwl.Extends.Utility;

namespace Uwl.Data.Server.UserServices
{
    /// <summary>
    /// Uwl.Data.Server为服务层
    /// 用户服务层实现
    /// </summary>
    public class UserServer : IUserServer
    {
        /// <summary>
        /// 定义领域仓储层的接口对象
        /// </summary>
        private readonly IUserRepositoty _userRepositoty;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepositoty _roleRepositoty;
        private readonly IUnitofWork _unitofWork;
        public UserServer(IUserRepositoty userRepositoty, IUserRoleRepository userRoleRepository, IRoleRepositoty roleRepositoty, 
            IUnitofWork unitofWork)
        {
            _userRepositoty = userRepositoty;
            _userRoleRepository = userRoleRepository;
            _roleRepositoty = roleRepositoty;
            this._unitofWork = unitofWork;
        }
        /// <summary>
        /// 调用仓储层的方法
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<SysUser> CheckUser(string userName, string password)
        {
            return await _userRepositoty.FirstOrDefaultAsync(t => t.Account == userName && t.Password == password);
        }
        #region
        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<SysUser> GetSysUser(Guid userid)
        {
            return await _userRepositoty.GetModelAsync(userid);
        }
        public List<SysUser> GetUserListByPage()
        {
            PageCriteria pageCriteria = new PageCriteria();
            StringBuilder sbWhere = new StringBuilder();
            sbWhere.Append(" UserName=@UserName");
            pageCriteria.ParamsList.Add(new ProcParamHelp
            {
                ParamName = "@UserName",
                ParamValue = "admin",
                ParamType = "varchar(50)",
            });
            sbWhere.Append(" and PassWord=@PassWord");
            pageCriteria.ParamsList.Add(new ProcParamHelp
            {
                ParamName = "@PassWord",
                ParamValue = "123456",
                ParamType = "varchar(50)",
            });
            pageCriteria.TableName = "Users";
            pageCriteria.PrimaryKey = "Users.Id";
            pageCriteria.Fields = "Users.UserName,Users.Password,Id";
            pageCriteria.PageIndex = 1;
            pageCriteria.PageSize = 15;
            pageCriteria.OrderBySort = " UserName desc ";
            pageCriteria.Wherecondition = sbWhere.ToString();
            //return  PageHelper.GetPageByParam<SysUser>(pageCriteria).ItemsList;
            return new List<SysUser>();
        }
        /// <summary>
        /// 存储过程调用分页
        /// </summary>
        /// <param name="userQuery"></param>
        /// <param name="Total"></param>
        /// <returns></returns>
        public List<SysUser> GetUserListByPage(UserQuery userQuery, out int Total)
        {
            var query = ExpressionBuilder.True<SysUser>();
            query = query.And(user => user.IsDrop == false);
            if(userQuery.stateEnum!= StateEnum.All)
            {
                query = query.And(user => user.AccountState == userQuery.stateEnum);
            }
            if (!userQuery.Mobile.IsNullOrEmpty())
            {
                query = query.And(user => user.Mobile.Contains(userQuery.Mobile.Trim()));
            }
            if (!userQuery.Name.IsNullOrEmpty())
            {
                query = query.And(user => user.Name.Contains(userQuery.Name.Trim()));
            }
            if (!userQuery.Account.IsNullOrEmpty())
            {
                query = query.And(user => user.Account.Contains(userQuery.Account.Trim()));
            }
            if (!userQuery.Account.IsNullOrEmpty())
            {
                query = query.And(user => user.Account.Contains(userQuery.Account.Trim()));
            }
            Total = _userRepositoty.Count(query);
            return _userRepositoty.PageBy(userQuery.PageIndex, userQuery.PageSize, query).ToList();
        }
        public List<SysUser> GetUsers(int pageIndex, int pageSize, out int total)
        {
            total = _userRepositoty.Count(t => t.Account == "admin");
            return _userRepositoty.PageBy(pageIndex,pageSize,t=>t.Account == "admin").ToList();
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="sysUser"></param>
        /// <returns></returns>
        public async Task<bool> AddUser(SysUser sysUser)
        {
            try
            {
                await _userRepositoty.InsertAsync(sysUser);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 查询出指定Id的菜单实体
        /// </summary>
        /// <param name="GuIds"></param>
        /// <returns></returns>
        public List<SysUser> GetAllListByWhere(List<Guid> sysUserIds)
        {
            return _userRepositoty.GetAll(x=> sysUserIds.Contains(x.Id)).ToList();
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="sysUser"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUser(SysUser sysUser)
        {
            sysUser.UpdateDate = DateTime.Now;
            try
            {
                await _userRepositoty.UpdateNotQueryAsync(sysUser,x=>x.Name, x => x.Sex, x => x.Email, 
                    x => x.Mobile, x => x.QQ, x => x.WeChat, x => x.EmpliyeeType, x => x.JobName, x => x.AccountState
                    , x => x.OrganizeId, x => x.DepartmentId, x => x.DepartmentId
                    );
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 批量删除对应的用户
        /// </summary>
        /// <param name="sysUsers"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUser(List<Guid> guids)
        {
            try
            {
                var list = GetAllListByWhere(guids);
                list.ForEach(x =>
                {
                    x.IsDrop = true;
                });
                return await _userRepositoty.UpdateAsync(list);
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
        /// <summary>
        /// 根据用户ID获取该用户下面的所有角色
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string> GetUserRoleByUserId(Guid userId)
        {
            try
            {
                string RoleName = "";
                var RoleIds = (await _userRoleRepository.GetAllListAsync(x => x.UserIdOrDepId == userId)).Select(x => x.RoleId);//用户角色对象仓储
                var RoleList = (await _roleRepositoty.GetAllListAsync(x => RoleIds.Contains(x.Id))).Select(x => x.Id).ToArray();//角色对象仓储
                if (RoleList.Any())
                {
                    RoleName = string.Join(',', RoleList);
                }
                return RoleName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
        /// <summary>
        /// 用户个人修改密码
        /// </summary>
        /// <param name="changePwd"></param>
        /// <returns></returns>
        public async Task<bool> ChangePwd(ChangePwdVO changePwd)
        {
            changePwd.oldPassWord = changePwd.oldPassWord.ToMD5();
            changePwd.newPassWord = changePwd.newPassWord.ToMD5();
            changePwd.passwdCheck = changePwd.passwdCheck.ToMD5();
            if (changePwd.UserId == Guid.Empty)
                throw new Exception("用户Id不存在,请重新登录!");
            if (changePwd.newPassWord != changePwd.passwdCheck)
                throw new Exception("两次输入的密码不一致请重新输入!");
            var model= await _userRepositoty.GetModelAsync(changePwd.UserId.Value);

            
            if (model==null)
                throw new Exception("用户信息不存在，不可修改密码!");
            if (changePwd.oldPassWord!= model.Password)
                throw new Exception("输入的旧密码对请重新输入!");
            model.Password = changePwd.newPassWord;
            try
            {
                return await _userRepositoty.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
        /// <summary>
        /// 用户个人资料修改
        /// </summary>
        /// <param name="sysUser"></param>
        /// <returns></returns>
        public async Task<bool> ChangeData(ChangeDataVO changeData)
        {
            try
            {
                var model = await _userRepositoty.GetModelAsync(changeData.UserId);
                if (model == null)
                    throw new Exception("用户信息不存在，不可修改密码!");
                model = MyMappers.ObjectMapper.Map<ChangeDataVO,SysUser>(changeData, model);                
                return await _userRepositoty.UpdateAsync(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
