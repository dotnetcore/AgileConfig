import { useIntl } from "@/.umi/plugin-locale/localeExports";
import { UserItem } from "@/pages/User/data";
import { adminUsers, allUsers } from "@/pages/User/service";
import {  ModalForm, ProFormSelect } from "@ant-design/pro-form";
import React, { useEffect, useState } from 'react';
import { AppListItem } from "../data";

export type UserAuthProps = {
    onSubmit: (values: AppListItem) => Promise<void>;
    onCancel: () => void;
    userAuthModalVisible: boolean;
    value: AppListItem | undefined ;
  };
const UserAuth : React.FC<UserAuthProps> = (props)=>{
    const intl = useIntl();
    const [users, setUsers] = useState<{lable:string, value:string}[]>();

    useEffect(()=>{
      allUsers().then( resp=>{
        const usermp = resp.data.map( (x: { userName: string, id: string, team: string })=> {
          return { label:x.userName + ' - ' + (x.team?x.team:''), value:x.id};
        });
        setUsers(usermp);
      });
    },[]);

    return (
    <ModalForm 
    title={props.value?.name + ' - 用户授权'}
    initialValues={props.value}
    visible={props.userAuthModalVisible}
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
    <ProFormSelect
                  mode="multiple"
                  label="配置修改权"
                  name="configEditUsers"
                  options={
                    users
                  }
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
                  label="配置上下线权"
                  name="configPublishUsers"
                  options={
                    users
                  }
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
    );
}

export default UserAuth;
