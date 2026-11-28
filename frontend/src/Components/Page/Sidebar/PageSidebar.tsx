import classNames from 'classnames';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import ReactDOM from 'react-dom';
import { useDispatch } from 'react-redux';
import { useLocation } from 'react-router';
import QueueStatus from 'Activity/Queue/Status/QueueStatus';
import { IconName } from 'Components/Icon';
import OverlayScroller from 'Components/Scroller/OverlayScroller';
import Scroller from 'Components/Scroller/Scroller';
import { icons } from 'Helpers/Props';
import { setIsSidebarVisible } from 'Store/Actions/appActions';
import dimensions from 'Styles/Variables/dimensions';
import HealthStatus from 'System/Status/Health/HealthStatus';
import translate from 'Utilities/String/translate';
import Messages from './Messages/Messages';
import PageSidebarItem from './PageSidebarItem';
import styles from './PageSidebar.css';

const HEADER_HEIGHT = parseInt(dimensions.headerHeight);
const SIDEBAR_WIDTH = parseInt(dimensions.sidebarWidth);

interface SidebarItem {
  iconName?: IconName;
  title: string | (() => string);
  to: string;
  alias?: string;
  isActive?: boolean;
  isActiveParent?: boolean;
  isParentItem?: boolean;
  isChildItem?: boolean;
  statusComponent?: React.ElementType;
  children?: {
    title: string | (() => string);
    to: string;
    statusComponent?: React.ElementType;
  }[];
}

const LINKS: SidebarItem[] = [
  {
    iconName: icons.MOVIE_CONTINUING,
    title: () => translate('Movies'),
    to: '/',
    alias: '/movies',
    children: [
      {
        title: () => translate('AddNew'),
        to: '/add/new',
      },
      {
        title: () => translate('ImportLibrary'),
        to: '/add/import',
      },
      {
        title: () => translate('Collections'),
        to: '/collections',
      },
      {
        title: () => translate('Discover'),
        to: '/add/discover',
      },
    ],
  },

  {
    iconName: icons.CALENDAR,
    title: () => translate('Calendar'),
    to: '/calendar',
  },

  {
    iconName: icons.ACTIVITY,
    title: () => translate('Activity'),
    to: '/activity/queue',
    children: [
      {
        title: () => translate('Queue'),
        to: '/activity/queue',
        statusComponent: QueueStatus,
      },
      {
        title: () => translate('History'),
        to: '/activity/history',
      },
      {
        title: () => translate('Blocklist'),
        to: '/activity/blocklist',
      },
    ],
  },

  {
    iconName: icons.WARNING,
    title: () => translate('Wanted'),
    to: '/wanted/missing',
    children: [
      {
        title: () => translate('Missing'),
        to: '/wanted/missing',
      },
      {
        title: () => translate('CutoffUnmet'),
        to: '/wanted/cutoffunmet',
      },
    ],
  },

  {
    iconName: icons.SETTINGS,
    title: () => translate('Settings'),
    to: '/settings',
    children: [
      {
        title: () => translate('MediaManagement'),
        to: '/settings/mediamanagement',
      },
      {
        title: () => translate('Profiles'),
        to: '/settings/profiles',
      },
      {
        title: () => translate('Quality'),
        to: '/settings/quality',
      },
      {
        title: () => translate('CustomFormats'),
        to: '/settings/customformats',
      },
      {
        title: () => translate('Indexers'),
        to: '/settings/indexers',
      },
      {
        title: () => translate('DownloadClients'),
        to: '/settings/downloadclients',
      },
      {
        title: () => translate('ImportLists'),
        to: '/settings/importlists',
      },
      {
        title: () => translate('Connect'),
        to: '/settings/connect',
      },
      {
        title: () => translate('Metadata'),
        to: '/settings/metadata',
      },
      {
        title: () => translate('Tags'),
        to: '/settings/tags',
      },
      {
        title: () => translate('General'),
        to: '/settings/general',
      },
      {
        title: () => translate('Ui'),
        to: '/settings/ui',
      },
    ],
  },

  {
    iconName: icons.SYSTEM,
    title: () => translate('System'),
    to: '/system/status',
    children: [
      {
        title: () => translate('Status'),
        to: '/system/status',
        statusComponent: HealthStatus,
      },
      {
        title: () => translate('Tasks'),
        to: '/system/tasks',
      },
      {
        title: () => translate('Backup'),
        to: '/system/backup',
      },
      {
        title: () => translate('Updates'),
        to: '/system/updates',
      },
      {
        title: () => translate('Events'),
        to: '/system/events',
      },
      {
        title: () => translate('LogFiles'),
        to: '/system/logs/files',
      },
    ],
  },
];

function hasActiveChildLink(link: SidebarItem, pathname: string) {
  const children = link.children;

  if (!children || !children.length) {
    return false;
  }

  return children.some((child) => {
    return child.to === pathname;
  });
}

interface PageSidebarProps {
  isSmallScreen: boolean;
  isSidebarVisible: boolean;
}

function PageSidebar({ isSidebarVisible, isSmallScreen }: PageSidebarProps) {
  const dispatch = useDispatch();
  const location = useLocation();
  const sidebarRef = useRef(null);
  const [sidebarStyle, setSidebarStyle] = useState({
    top: dimensions.headerHeight,
    height: `${window.innerHeight - HEADER_HEIGHT}px`,
  });

  const urlBase = window.Radarr.urlBase;
  const pathname = urlBase
    ? location.pathname.substr(urlBase.length) || '/'
    : location.pathname;

  const activeParent = useMemo(() => {
    return (
      LINKS.find((link) => {
        if (link.to && link.to === pathname) {
          return true;
        }

        const children = link.children;

        if (children) {
          const matchingChild = children.find((childLink) => {
            return pathname.startsWith(childLink.to);
          });

          if (matchingChild) {
            return matchingChild;
          }
        }

        if (
          (link.to !== '/' && pathname.startsWith(link.to)) ||
          (link.alias && pathname.startsWith(link.alias))
        ) {
          return true;
        }

        return false;
      })?.to ?? LINKS[0].to
    );
  }, [pathname]);

  const handleWindowClick = useCallback(
    (event: MouseEvent) => {
      const sidebar = ReactDOM.findDOMNode(sidebarRef.current);
      const toggleButton = document.getElementById('sidebar-toggle-button');
      const target = event.target;

      if (!sidebar) {
        return;
      }

      if (
        target instanceof Node &&
        !sidebar.contains(target) &&
        !toggleButton?.contains(target) &&
        isSidebarVisible
      ) {
        event.preventDefault();
        event.stopPropagation();
        dispatch(setIsSidebarVisible({ isSidebarVisible: false }));
      }
    },
    [isSidebarVisible, dispatch]
  );

  const handleItemPress = useCallback(() => {
    dispatch(setIsSidebarVisible({ isSidebarVisible: false }));
  }, [dispatch]);

  const handleWindowScroll = useCallback(() => {
    const windowScroll =
      window.scrollY == null
        ? document.documentElement.scrollTop
        : window.scrollY;
    const sidebarTop = Math.max(HEADER_HEIGHT - windowScroll, 0);
    const sidebarHeight = window.innerHeight - sidebarTop;

    if (isSmallScreen) {
      setSidebarStyle({
        top: `${sidebarTop}px`,
        height: `${sidebarHeight}px`,
      });
    }
  }, [isSmallScreen]);

  useEffect(() => {
    if (isSmallScreen) {
      window.addEventListener('click', handleWindowClick, { capture: true });
      window.addEventListener('scroll', handleWindowScroll);
    }

    return () => {
      window.removeEventListener('click', handleWindowClick, { capture: true });
      window.removeEventListener('scroll', handleWindowScroll);
    };
  }, [isSmallScreen, handleWindowClick, handleWindowScroll]);

  const containerStyle = useMemo(() => {
    if (!isSmallScreen) {
      return undefined;
    }

    return {
      transition: 'none',
      transform: `translateX(${isSidebarVisible ? 0 : -SIDEBAR_WIDTH}px)`,
    };
  }, [isSmallScreen, isSidebarVisible]);

  const ScrollerComponent = isSmallScreen ? Scroller : OverlayScroller;

  return (
    <div
      ref={sidebarRef}
      className={classNames(styles.sidebarContainer)}
      style={containerStyle}
    >
      <ScrollerComponent
        className={styles.sidebar}
        scrollDirection="vertical"
        style={sidebarStyle}
      >
        <div>
          {LINKS.map((link) => {
            const childWithStatusComponent = link.children?.find((child) => {
              return !!child.statusComponent;
            });

            const childStatusComponent = childWithStatusComponent
              ? childWithStatusComponent.statusComponent
              : null;

            const isActiveParent = activeParent === link.to;
            const hasActiveChild = hasActiveChildLink(link, pathname);

            return (
              <PageSidebarItem
                key={link.to}
                iconName={link.iconName}
                title={link.title}
                to={link.to}
                statusComponent={
                  isActiveParent || !childStatusComponent
                    ? link.statusComponent
                    : childStatusComponent
                }
                isActive={pathname === link.to && !hasActiveChild}
                isActiveParent={isActiveParent}
                isParentItem={!!link.children}
                onPress={handleItemPress}
              >
                {link.children &&
                  link.to === activeParent &&
                  link.children.map((child) => {
                    return (
                      <PageSidebarItem
                        key={child.to}
                        title={child.title}
                        to={child.to}
                        isActive={pathname === child.to}
                        isParentItem={false}
                        isChildItem={true}
                        statusComponent={child.statusComponent}
                        onPress={handleItemPress}
                      />
                    );
                  })}
              </PageSidebarItem>
            );
          })}
        </div>

        <Messages />
      </ScrollerComponent>
    </div>
  );
}

export default PageSidebar;
