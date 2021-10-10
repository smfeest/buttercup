import {
  createPopper,
  Instance as PopperInstance,
  Options as PopperOptions,
} from '@popperjs/core';

export type PopoverMenuOptions = Partial<PopperOptions>;

export type PopoverMenu = {
  close: () => void;
  destroy: () => void;
  isOpen: () => boolean;
  open: () => void;
};

export const createPopoverMenu = (
  document: Document,
  button: HTMLElement,
  popover: HTMLElement,
  popoverOptions?: PopoverMenuOptions
): PopoverMenu => {
  let popper: PopperInstance | null = null;

  const close = () => {
    if (popper) {
      popover.classList.remove('popover-menu--open');
      setExpanded(false);

      popper.destroy();
      popper = null;

      document.removeEventListener('click', onDocumentClick);
    }
  };

  const destroy = () => {
    close();

    button.removeEventListener('click', onButtonClick);
    button.removeEventListener('keydown', onKeyDown);

    popover.removeEventListener('keydown', onKeyDown);
  };

  const isOpen = () => !!popper;

  const open = () => {
    if (!popper) {
      popper = createPopper(button, popover, popoverOptions);

      popover.classList.add('popover-menu--open');
      setExpanded(true);

      document.addEventListener('click', onDocumentClick);
    }
  };

  const onButtonClick = () => (popper ? close() : open());

  const onDocumentClick = (event: MouseEvent) => {
    const { defaultPrevented, target } = event;

    if (
      !defaultPrevented &&
      target instanceof Node &&
      !button.contains(target) &&
      !popover.contains(target)
    ) {
      close();
      event.preventDefault();
    }
  };

  const onKeyDown = (event: KeyboardEvent) => {
    const { key, shiftKey, target } = event;

    if (popper) {
      const shiftFocus = (offset: number) => {
        const items = Array.from(popover.getElementsByTagName('a'));

        if (items.length > 0) {
          let targetIndex = items.indexOf(target as HTMLAnchorElement) + offset;
          const maxIndex = items.length - 1;

          if (targetIndex < 0) {
            targetIndex = maxIndex;
          } else if (targetIndex > maxIndex) {
            targetIndex = 0;
          }

          items[targetIndex].focus();
        }

        event.preventDefault();
      };

      switch (key) {
        case 'Escape':
          button.focus();
          close();
          break;
        case 'ArrowUp':
          shiftFocus(-1);
          break;
        case 'ArrowDown':
          shiftFocus(1);
          break;
        case 'Tab':
          shiftFocus(shiftKey ? -1 : 1);
          break;
      }
    } else if (key === 'ArrowUp' || key === 'ArrowDown') {
      open();
      event.preventDefault();
    }
  };

  const setExpanded = (expanded: boolean) =>
    button.setAttribute('aria-expanded', expanded ? 'true' : 'false');

  if (!button.id) {
    let i = 0;
    let id: string;

    do {
      id = `popover-menu-button-${i++}`;
    } while (document.getElementById(id));

    button.id = id;
  }

  button.setAttribute('aria-haspopup', 'true');
  button.addEventListener('click', onButtonClick);
  button.addEventListener('keydown', onKeyDown);

  popover.setAttribute('aria-labelledby', button.id);
  popover.addEventListener('keydown', onKeyDown);

  setExpanded(false);

  return {
    close,
    destroy,
    isOpen,
    open,
  };
};
