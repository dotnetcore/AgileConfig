import { BulbFilled, BulbOutlined } from '@ant-design/icons';
import { Tag, Tooltip } from 'antd';
import type { Settings as ProSettings } from '@ant-design/pro-layout';
import React from 'react';
import type { ConnectProps } from 'umi';
import { connect, SelectLang, useIntl } from 'umi';
import type { ConnectState } from '@/models/connect';
import Avatar from './AvatarDropdown';
import styles from './index.less';

export type GlobalHeaderRightProps = {
  theme?: ProSettings['navTheme'] | 'realDark';
  darkMode?: boolean;
} & Partial<ConnectProps> &
  Partial<ProSettings>;

const ENVTagColor = {
  dev: 'orange',
  test: 'green',
  pre: '#87d068',
};

const GlobalHeaderRight: React.SFC<GlobalHeaderRightProps> = (props) => {
  const { theme, layout } = props;
  const { formatMessage } = useIntl();
  const darkMode = !!props.darkMode;
  let className = styles.right;

  if (theme === 'dark' && layout === 'top') {
    className = `${styles.right}  ${styles.dark}`;
  }

  return (
    <div className={className}>
      <Tooltip
        title={formatMessage({
          id: darkMode ? 'component.globalHeader.theme.light' : 'component.globalHeader.theme.dark',
        })}
      >
        <span
          aria-label={formatMessage({
            id: darkMode ? 'component.globalHeader.theme.light' : 'component.globalHeader.theme.dark',
          })}
          className={styles.action}
          role="button"
          tabIndex={0}
          onClick={() => props.dispatch?.({ type: 'settings/changeSetting', payload: { darkMode: !darkMode } })}
          onKeyDown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              event.preventDefault();
              props.dispatch?.({ type: 'settings/changeSetting', payload: { darkMode: !darkMode } });
            }
          }}
        >
          {darkMode ? <BulbFilled /> : <BulbOutlined />}
        </span>
      </Tooltip>
      <Avatar />
      {REACT_APP_ENV && (
        <span>
          <Tag color={ENVTagColor[REACT_APP_ENV]}>{REACT_APP_ENV}</Tag>
        </span>
      )}
      <SelectLang className={styles.action} />
    </div>
  );
};

export default connect(({ settings }: ConnectState) => ({
  theme: settings.darkMode ? 'dark' : settings.navTheme,
  layout: settings.layout,
  darkMode: settings.darkMode,
}))(GlobalHeaderRight);
