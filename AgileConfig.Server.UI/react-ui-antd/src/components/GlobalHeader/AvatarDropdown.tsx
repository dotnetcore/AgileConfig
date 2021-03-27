import { LogoutOutlined, SettingOutlined, } from '@ant-design/icons';
import { Menu, Spin } from 'antd';
import React from 'react';
import type { ConnectProps } from 'umi';
import {  connect,getIntl, getLocale } from 'umi';
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
    const intl = getIntl(getLocale());
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
            {
             intl.formatMessage({
              id: 'menu.account.resetPassword'
             })
            }
        </Menu.Item>
        <Menu.Item key="logout">
          <LogoutOutlined />
           {
             intl.formatMessage({
              id: 'menu.account.logout'
             })
           }
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
            <span className={`${styles.name} anticon`}>{
              <svg style={{
                width:"25px",
                height: "25px"
              }}  viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="2344" width="200" height="200"><path d="M508.289087 551.759785h6.78567c62.13129-1.060261 112.387658-22.901636 149.49679-64.675917 81.640091-92.030648 68.068751-249.797474 66.584386-264.853178-5.301305-113.023814-58.738455-167.097122-102.84531-192.331332C595.442535 11.026714 557.061089 0.848209 514.226548 0H510.621661c-23.537793 0-69.765169 3.816939-114.084076 29.051149-44.530959 25.23421-98.816318 79.307517-104.117622 193.179541-1.484365 15.055705-15.055705 172.822531 66.584386 264.853178 36.89708 41.77428 87.153448 63.615655 149.284738 64.675917z m-159.251191-324.227791c0-0.636157 0.212052-1.272313 0.212052-1.696417 6.997722-152.041416 114.932284-168.369435 161.159661-168.369435H512.954235c57.25409 1.272313 154.586043 24.598053 161.15966 168.369435 0 0.636157 0 1.272313 0.212052 1.696417 0.212052 1.484365 15.055705 145.679851-52.376889 221.594533-26.718575 30.11141-62.343342 44.955063-109.206875 45.379168h-2.120522c-46.651481-0.424104-82.4883-15.267757-108.994823-45.379168-67.220543-75.490578-52.800994-220.32222-52.588942-221.594533z" p-id="2345" fill="#ffffff"></path><path d="M947.449161 813.432181v-0.636157c0-1.696417-0.212052-3.392835-0.212052-5.301305-1.272313-41.986333-4.028992-140.166494-96.05964-171.550217-0.636157-0.212052-1.484365-0.424104-2.120521-0.636157-95.635535-24.386001-175.155105-79.519569-176.003314-80.155725-12.935183-9.118244-30.747567-5.937461-39.86581 6.997722-9.118244 12.935183-5.937461 30.747567 6.997722 39.86581 3.604887 2.544626 88.001657 61.283081 193.603644 88.425761 49.408159 17.600331 54.921516 70.401325 56.405881 118.749224 0 1.90847 0 3.604887 0.212053 5.301304 0.212052 19.084697-1.060261 48.55995-4.453096 65.524126-34.352454 19.508801-169.005591 86.941396-373.848002 86.941395-203.994202 0-339.495548-67.644647-374.060054-87.153448-3.392835-16.964175-4.8772-46.439428-4.453096-65.524125 0-1.696417 0.212052-3.392835 0.212053-5.301304 1.484365-48.347898 6.997722-101.148892 56.405881-118.749224 105.601988-27.14268 189.998758-86.093187 193.603644-88.425761 12.935183-9.118244 16.115966-26.930627 6.997722-39.86581-9.118244-12.935183-26.930627-16.115966-39.86581-6.997723-0.848209 0.636157-79.943674 55.769725-176.003314 80.155726-0.848209 0.212052-1.484365 0.424104-2.120521 0.636157-92.030648 31.595776-94.787327 129.775937-96.05964 171.550217 0 1.90847 0 3.604887-0.212052 5.301305v0.636156c-0.212052 11.026714-0.424104 67.644647 10.814661 96.05964 2.120522 5.513357 5.937461 10.178505 11.026714 13.359288 6.361566 4.241044 158.827086 101.360944 413.925864 101.360944s407.564299-97.331953 413.925865-101.360944c4.8772-3.180783 8.906192-7.845931 11.026713-13.359288 10.602609-28.202941 10.390557-84.820874 10.178505-95.847587z" p-id="2346" fill="#ffffff"></path></svg>

            }</span>
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
