import { PageContainer } from '@ant-design/pro-layout';
import { Spin } from 'antd';
import styles from './index.less';
import { oidcLogin } from '@/services/login';
import Model from '@/models/login';
import { history,getIntl,getLocale } from 'umi';
import { message } from 'antd';

const logs:React.FC = (props: any) =>  {
  const intl = getIntl(getLocale());
  const search = props.location.search; // 获取 URL 中的查询字符串，如 "?foo=bar"
  const params = new URLSearchParams(search); // 使用 URLSearchParams 解析查询字符串
  const code = params.get('code'); 
  console.log('OIDC code', code);
  if (code) {
    const loginModel =  Model;
    oidcLogin(code).then(( response )=>{
      if(response.status === 'ok') {
        const msg = intl.formatMessage({
          id: 'pages.login.loginsuccess'
        });
        message.success(msg);
        loginModel.reducers.changeLoginStatus({}, {payload: response});
        history.replace('/');
      }
      else {
        const msg = intl.formatMessage({
          id: 'pages.login.loginfail'
        });
        message.error(msg);
        loginModel.effects.logout();
      }
    });
  }
  return (
    <PageContainer>
      <div className={styles.loading}>
        <Spin tip="OIDC loading..." size='large'>
        </Spin>
      </div>
    </PageContainer>
  );
}
export default logs;
