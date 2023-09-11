import { useIntl } from "@/.umi/plugin-locale/localeExports";
import { checkUserPermission } from "@/components/Authorized/AuthorizedElement";
import functionKeys from "@/models/functionKeys";
import { allUsers } from "@/pages/User/service";
import { getFunctions } from "@/utils/authority";
import { ModalForm, ProFormSelect, ProFormText } from "@ant-design/pro-form";
import React, { useEffect, useState } from 'react';
import { AppListItem, UserAppAuth } from "../data";
import { getUserAppAuth } from "../service";

export type UserAuthProps = {
    onSubmit: (values: UserAppAuth) => Promise<void>;
    onCancel: () => void;
    userAuthModalVisible: boolean;
    value: AppListItem | undefined ;
  };
const UserAuth : React.FC<UserAuthProps> = (props)=>{
    const intl = useIntl();
    const [users, setUsers] = useState<{lable:string, value:string}[]>();
    const [userAppAuthState, setUserAppAuthState] = useState<UserAppAuth>();

    useEffect(()=>{
      allUsers().then( resp=>{
        const usermp = resp.data.map( (x: { userName: string, id: string, team: string })=> {
          return { label:x.userName + ' - ' + (x.team?x.team:''), value:x.id};
        });
        setUsers(usermp);
      });
    },[]);
    useEffect(()=>{
      if (props.value?.id) {
        const appId = props.value.id
        getUserAppAuth(appId).then(resp => {
          var auth:UserAppAuth = {
            appId: appId,
            editConfigPermissionUsers : resp.data.editConfigPermissionUsers,
            publishConfigPermissionUsers: resp.data.publishConfigPermissionUsers
          };
          setUserAppAuthState(auth);
        });
      }
    },[props.value?.id]);
    return (
      userAppAuthState ?
    <ModalForm 
    title={props.value?.name + ' - 用户授权'}
    initialValues={userAppAuthState}
    visible={props.userAuthModalVisible}
    submitter = {
      checkUserPermission(getFunctions(),functionKeys.App_Auth, props.value?.id)?{
        submitButtonProps:{}
      }:{
        submitButtonProps:{style:{display:"none"}}
      }
    }
    modalProps={
      {
        onCancel: ()=>{
          props.onCancel();
        }
      }
    }
     onFinish={
       props.onSubmit
    }
    >
    <ProFormText
      hidden={true}
      readonly={true}
      name="appId"
    />
    <ProFormSelect
                  mode="multiple"
                  label="修改权"
                  name="editConfigPermissionUsers"
                  options={
                    users
                  }
                  readonly={!checkUserPermission(getFunctions(),functionKeys.App_Auth, props.value?.id)}
                  fieldProps={
                    {
                      filterOption:(item, option)=>{
                        const label = option?.label?.toString();
                        if (item && label) {
                          return label.indexOf(item) >= 0
                        }
                        return false;
                      }
                    }
                  }
        >
    </ProFormSelect>    
    <ProFormSelect
                  mode="multiple"
                  label="发布权"
                  name="publishConfigPermissionUsers"
                  options={
                    users
                  }
                  readonly={!checkUserPermission(getFunctions(),functionKeys.App_Auth, props.value?.id)}
                  fieldProps={
                    {
                      filterOption:(item, option)=>{
                        const label = option?.label?.toString();
                        if (item && label) {
                          return label.indexOf(item) >= 0
                        }
                        return false;
                      }
                    }
                  }
        >
    </ProFormSelect>
    </ModalForm>
    :
    <div></div>
    );
}

export default UserAuth;
