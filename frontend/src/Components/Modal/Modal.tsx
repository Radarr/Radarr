import classNames from 'classnames';
import elementClass from 'element-class';
import React, {
  MouseEvent,
  useCallback,
  useEffect,
  useId,
  useRef,
} from 'react';
import ReactDOM from 'react-dom';
import FocusLock from 'react-focus-lock';
import ErrorBoundary from 'Components/Error/ErrorBoundary';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { Size } from 'Helpers/Props/sizes';
import { isMobile } from 'Utilities/browser';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import { setScrollLock } from 'Utilities/scrollLock';
import ModalError from './ModalError';
import styles from './Modal.css';

const openModals: string[] = [];
const node = document.getElementById('portal-root');

function removeFromOpenModals(id: string) {
  const index = openModals.indexOf(id);

  if (index >= 0) {
    openModals.splice(index, 1);
  }
}

// Mobile scroll lock: block touchmove outside the modal portal. Avoids
// mutating body position/overflow — doing so resets window.scrollY to 0,
// which causes react-virtualized WindowScroller to unmount the row that
// owns the just-opened modal (Discover page regression).
function preventTouchScroll(event: TouchEvent) {
  let target = event.target as HTMLElement | null;
  while (target) {
    if (target.id === 'portal-root') {
      return;
    }
    target = target.parentElement;
  }
  event.preventDefault();
}

function findEventTarget(event: TouchEvent | MouseEvent) {
  if ('changedTouches' in event) {
    const changedTouches = event.changedTouches;

    if (!changedTouches) {
      return event.target;
    }

    if (changedTouches.length === 1) {
      const touch = changedTouches[0];

      return document.elementFromPoint(touch.clientX, touch.clientY);
    }
  }

  return event.target;
}

export interface ModalProps {
  className?: string;
  style?: object;
  backdropClassName?: string;
  size?: Extract<Size, keyof typeof styles>;
  children?: React.ReactNode;
  isOpen: boolean;
  closeOnBackgroundClick?: boolean;
  onModalClose: () => void;
}

function Modal({
  className = styles.modal,
  style,
  backdropClassName = styles.modalBackdrop,
  size = 'large',
  children,
  isOpen,
  closeOnBackgroundClick = true,
  onModalClose,
}: ModalProps) {
  const backgroundRef = useRef<HTMLDivElement>(null);
  const isBackdropPressed = useRef(false);
  const wasOpen = usePrevious(isOpen);
  const modalId = useId();

  const isTargetBackdrop = useCallback((event: TouchEvent | MouseEvent) => {
    const targetElement = findEventTarget(event);

    if (targetElement) {
      return backgroundRef.current?.isEqualNode(targetElement as Node) ?? false;
    }

    return false;
  }, []);

  const handleBackdropBeginPress = useCallback(
    (event: MouseEvent<HTMLDivElement>) => {
      isBackdropPressed.current = isTargetBackdrop(event);
    },
    [isTargetBackdrop]
  );

  const handleBackdropEndPress = useCallback(
    (event: MouseEvent<HTMLDivElement>) => {
      if (
        isBackdropPressed.current &&
        isTargetBackdrop(event) &&
        closeOnBackgroundClick
      ) {
        onModalClose();
      }

      isBackdropPressed.current = false;
    },
    [closeOnBackgroundClick, isTargetBackdrop, onModalClose]
  );

  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      if (event.keyCode === keyCodes.ESCAPE) {
        if (openModals.indexOf(modalId) === openModals.length - 1) {
          event.preventDefault();
          event.stopPropagation();

          onModalClose();
        }
      }
    },
    [modalId, onModalClose]
  );

  useEffect(() => {
    if (isOpen && !wasOpen) {
      openModals.push(modalId);

      if (openModals.length === 1) {
        if (isMobile()) {
          // Don't mutate body position/overflow on mobile — it resets
          // window.scrollY which makes WindowScroller unmount this row's
          // modal. Use a touchmove listener to block scroll instead.
          setScrollLock(true);
          document.addEventListener('touchmove', preventTouchScroll, {
            passive: false,
          });
        } else {
          elementClass(document.body).add(styles.modalOpen);
        }
      }
    } else if (!isOpen && wasOpen) {
      removeFromOpenModals(modalId);

      if (openModals.length === 0) {
        setScrollLock(false);

        if (isMobile()) {
          document.removeEventListener('touchmove', preventTouchScroll);
        } else {
          elementClass(document.body).remove(styles.modalOpen);
        }
      }
    }
  }, [isOpen, wasOpen, modalId, handleKeyDown]);

  useEffect(() => {
    if (isOpen) {
      window.addEventListener('keydown', handleKeyDown);
    }

    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, handleKeyDown]);

  if (!isOpen) {
    return null;
  }

  return ReactDOM.createPortal(
    <FocusLock disabled={false}>
      <div className={styles.modalContainer}>
        <div
          ref={backgroundRef}
          className={backdropClassName}
          onMouseDown={handleBackdropBeginPress}
          onMouseUp={handleBackdropEndPress}
        >
          <div className={classNames(className, styles[size])} style={style}>
            <ErrorBoundary
              errorComponent={ModalError}
              onModalClose={onModalClose}
            >
              {children}
            </ErrorBoundary>
          </div>
        </div>
      </div>
    </FocusLock>,
    node!
  );
}

export default Modal;
