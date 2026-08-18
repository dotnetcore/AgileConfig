import { LogoutOutlined, SettingOutlined, } from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { Spin } from 'antd';
import React from 'react';
import type { ConnectProps } from '@umijs/max';
import {  connect,getIntl, getLocale } from '@umijs/max';
import type { ConnectState } from '@/models/connect';
import type { CurrentUser } from '@/models/user';
import HeaderDropdown from '../HeaderDropdown';
import styles from './index.less';
import Changepassword from '../ChangePassword/changePassword';
import avatar from '../../assets/avatar.png'

export type GlobalHeaderRightProps = {
  currentUser?: CurrentUser;
  menu?: boolean;
} & Partial<ConnectProps>;

class AvatarDropdown extends React.Component<GlobalHeaderRightProps,{changePasswordModalVisible: boolean}> {
  constructor(props:any) {
    super(props);
    this.state = {changePasswordModalVisible: false};
  }
  onMenuClick: MenuProps['onClick'] = (event) => {
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
    const intl = getIntl(getLocale());
    const {
      currentUser = {
        avatar: '',
        name: '',
      },
    } = this.props;
    const menuItems: MenuProps['items'] = [
      {
        key: 'resetPassword',
        icon: <SettingOutlined />,
        label: intl.formatMessage({ id: 'menu.account.resetPassword' }),
      },
      {
        key: 'logout',
        icon: <LogoutOutlined />,
        label: intl.formatMessage({ id: 'menu.account.logout' }),
      },
    ];
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
          
        <HeaderDropdown
          menu={{ items: menuItems, onClick: this.onMenuClick, selectedKeys: [] }}
          overlayClassName={styles.menu}
        >
          <span className={`${styles.action} ${styles.account}`}>
            <span className={`${styles.name} anticon`}>
            <img 
            style={
              {
                height:30,
                width:30
              }
            }
                 src={avatar}></img>
                 {
                   currentUser?.name
                 }
            </span>
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
