import { LogoutOutlined, SettingOutlined, } from '@ant-design/icons';
import { Menu, Spin } from 'antd';
import React from 'react';
import type { ConnectProps } from 'umi';
import {  connect } from 'umi';
import type { ConnectState } from '@/models/connect';
import type { CurrentUser } from '@/models/user';
import HeaderDropdown from '../HeaderDropdown';
import styles from './index.less';
import Changepassword from '../ChangePassword/changePassword';

export type GlobalHeaderRightProps = {
  currentUser?: CurrentUser;
  menu?: boolean;
} & Partial<ConnectProps>;

class AvatarDropdown extends React.Component<GlobalHeaderRightProps,{changePasswordModalVisible: boolean}> {
  constructor(props:any) {
    super(props);
    this.state = {changePasswordModalVisible: false};
  }
  onMenuClick = (event: {
    key: React.Key;
    keyPath: React.Key[];
    item: React.ReactInstance;
    domEvent: React.MouseEvent<HTMLElement>;
  }) => {
    const { key } = event;

    if (key === 'logout') {
      const { dispatch } = this.props;

      if (dispatch) {
        dispatch({
          type: 'login/logout',
        });
      }

      return;
    }
    if (key === 'resetPassword') {
      this.setState({
        changePasswordModalVisible: true
      });
    }
  };

  render(): React.ReactNode {
    const {
      currentUser = {
        avatar: '',
        name: '',
      },
      menu,
    } = this.props;
    const menuHeaderDropdown = (
      <Menu className={styles.menu} selectedKeys={[]} onClick={this.onMenuClick}>
        <Menu.Item key="resetPassword">
          <SettingOutlined />
            修改密码
        </Menu.Item>
        <Menu.Item key="logout">
          <LogoutOutlined />
          退出登录
        </Menu.Item>
      </Menu>
    );
    return (

      currentUser && currentUser.name ? (
        <div>
          {
            this.state.changePasswordModalVisible &&
            <Changepassword
              onSuccess={
                ()=>{
                  const { dispatch } = this.props;
                  if (dispatch) {
                    dispatch({
                      type: 'login/logout',
                    });
                  }
                }
              }
              onCancel={
                ()=>{
                  this.setState({
                    changePasswordModalVisible: false
                  });
                }
              }
              changePasswordModalVisible={this.state.changePasswordModalVisible}>
            </Changepassword>
          }
          
        <HeaderDropdown overlay={menuHeaderDropdown}>
          <span className={`${styles.action} ${styles.account}`}>
            <span className={`${styles.name} anticon`}>{currentUser.name}</span>
          </span>
        </HeaderDropdown>
        </div>
      ) : (
        <span className={`${styles.action} ${styles.account}`}>
          <Spin
            size="small"
            style={{
              marginLeft: 8,
              marginRight: 8,
            }}
          />
        </span>
      )
    );
  }
}

export default connect(({ user }: ConnectState) => ({
  currentUser: user.currentUser,
}))(AvatarDropdown);
