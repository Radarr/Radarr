import classNames from 'classnames';
import React, { Children, useCallback } from 'react';
import Icon, { IconName } from 'Components/Icon';
import Link from 'Components/Link/Link';
import styles from './PageSidebarItem.css';

export interface PageSidebarItemProps {
  iconName?: IconName;
  title: string | (() => string);
  to: string;
  isActive?: boolean;
  isActiveParent?: boolean;
  isParentItem?: boolean;
  isChildItem?: boolean;
  statusComponent?: React.ElementType;
  children?: React.ReactNode;
  onPress?: () => void;
  isCollapsed?: boolean;
}

function PageSidebarItem({
  iconName,
  title,
  to,
  isActive,
  isActiveParent,
  isChildItem = false,
  isParentItem = false,
  statusComponent: StatusComponent,
  children,
  onPress,
  isCollapsed = false,
}: PageSidebarItemProps) {
  const handlePress = useCallback(() => {
    if (isChildItem || !isParentItem) {
      onPress?.();
    }
  }, [isChildItem, isParentItem, onPress]);

  return (
    <div
      className={classNames(styles.item, isActiveParent && styles.isActiveItem, isCollapsed && styles.isCollapsed)}
    >
      <Link
        className={classNames(
          isChildItem ? styles.childLink : styles.link,
          isActiveParent && styles.isActiveParentLink,
          isActive && styles.isActiveLink
        )}
        to={to}
        onPress={handlePress}
        title={isCollapsed ? (typeof title === 'function' ? title() : title) : undefined}
        aria-label={isCollapsed ? (typeof title === 'function' ? title() : title) : undefined}
      >
        {!!iconName && (
          <span className={styles.iconContainer}>
            <Icon name={iconName} />
          </span>
        )}

        <span className={styles.label}>{typeof title === 'function' ? title() : title}</span>

        {!!StatusComponent && (
          <span className={styles.status}>
            <StatusComponent />
          </span>
        )}
      </Link>

      {!isCollapsed && children
        ? Children.map(children, (child) => {
            if (!React.isValidElement(child)) {
              return child;
            }

            const childProps = { isChildItem: true };

            return React.cloneElement(child, childProps);
          })
        : null}
    </div>
  );
}

export default PageSidebarItem;
