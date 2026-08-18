import type { DropdownProps } from 'antd';
import { Dropdown } from 'antd';
import React from 'react';
import classNames from 'classnames';
import styles from './index.less';

export type HeaderDropdownProps = DropdownProps;

const HeaderDropdown: React.FC<HeaderDropdownProps> = ({
  overlayClassName: cls,
  rootClassName,
  ...restProps
}) => (
  <Dropdown
    rootClassName={classNames(styles.container, cls, rootClassName)}
    {...restProps}
  />
);

export default HeaderDropdown;
