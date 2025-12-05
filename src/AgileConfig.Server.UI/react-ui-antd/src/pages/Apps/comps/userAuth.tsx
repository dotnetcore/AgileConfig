import { useIntl } from "@/.umi/plugin-locale/localeExports";
import { checkUserPermission } from "@/components/Authorized/AuthorizedElement";
import functionKeys from "@/models/functionKeys";
import { allUsers } from "@/pages/User/service";
import { getFunctions } from "@/utils/authority";
import { ModalForm, ProFormSelect, ProFormText } from "@ant-design/pro-form";
import React, { useEffect, useState } from 'react';
import { AppListItem, UserAppAuth } from "../data";
import { getUserAppAuth } from "../service";

// Unified interface definition indentation
export type UserAuthProps = {
  onSubmit: (values: UserAppAuth) => Promise<void>;
  onCancel: () => void;
  userAuthModalVisible: boolean;
  value?: AppListItem;
};

const UserAuth: React.FC<UserAuthProps> = (props) => {
  const intl = useIntl();
  const [users, setUsers] = useState<{ label: string; value: string }[]>();
  const [userAppAuthState, setUserAppAuthState] = useState<UserAppAuth>();
  const canEditAuth = checkUserPermission(getFunctions(), functionKeys.App_Auth, props.value?.id);

  // Optimize useEffect indentation
  useEffect(() => {
    const creatorId = props.value?.creator;
    allUsers().then((resp) => {
      const usermp = resp.data
        .filter((x: { id: string }) => x.id !== creatorId)
        .map((x: { userName: string; id: string; team: string }) => {
          return { label: x.userName + ' - ' + (x.team ? x.team : ''), value: x.id };
        });
      setUsers(usermp);
    });
  }, [props.value?.creator]);

  useEffect(() => {
    if (props.value?.id) {
      const appId = props.value.id;
      getUserAppAuth(appId).then((resp) => {
        var auth: UserAppAuth = {
          appId: appId,
          authorizedUsers: resp.data?.authorizedUsers ?? [],
        };
        setUserAppAuthState(auth);
      });
    }
  }, [props.value?.id]);

  // Unified JSX attributes indentation
  return userAppAuthState ? (
    <ModalForm
      title={props.value?.name + ' - ' + intl.formatMessage({ id: 'pages.app.auth.title' })}
      initialValues={userAppAuthState}
      visible={props.userAuthModalVisible}
      submitter={
        canEditAuth ? {
          submitButtonProps: {},
        } : {
          submitButtonProps: { style: { display: 'none' } },
        }
      }
      modalProps={{
        onCancel: () => {
          props.onCancel();
        },
      }}
      onFinish={props.onSubmit}
    >
      <ProFormText hidden={true} readonly={true} name="appId" />

      <ProFormSelect
        mode="multiple"
        label={intl.formatMessage({ id: 'pages.app.auth.bind_users' })}
        name="authorizedUsers"
        options={users}
        readonly={!canEditAuth}
        fieldProps={{
          filterOption: (item, option) => {
            const label = option?.label?.toString();
            if (item && label) {
              return label.indexOf(item) >= 0;
            }
            return false;
          },
        }}
      />
    </ModalForm>
  ) : (
    <div></div>
  );
};

export default UserAuth;
