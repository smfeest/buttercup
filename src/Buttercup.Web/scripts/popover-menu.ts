import {
  createPopper,
  Instance as PopperInstance,
  Options as PopperOptions,
} from '@popperjs/core';

export type Options = Partial<PopperOptions>;

export default class PopoverMenu {
  private popper: PopperInstance | null = null;

  public constructor(
    public document: Document,
    public button: HTMLElement,
    public popover: HTMLElement,
    public popoverOptions?: Options,
  ) {
    if (!button.id) {
      let i = 0;
      let id: string;

      do {
        id = `popover-menu-button-${i++}`;
      } while (document.getElementById(id));

      button.id = id;
    }

    button.setAttribute('aria-haspopup', 'true');
    button.addEventListener('click', this.onButtonClick);
    button.addEventListener('keydown', this.onKeyDown);

    popover.setAttribute('aria-labelledby', button.id);
    popover.addEventListener('keydown', this.onKeyDown);

    this.setExpanded(false);
  }

  public get isOpen() {
    return !!this.popper;
  }

  public close() {
    if (this.popper) {
      this.popover.classList.remove('popover-menu--open');
      this.setExpanded(false);

      this.popper.destroy();
      this.popper = null;

      this.document.removeEventListener('click', this.onDocumentClick);
    }
  }

  public destroy() {
    this.close();

    this.button.removeEventListener('click', this.onButtonClick);
    this.button.removeEventListener('keydown', this.onKeyDown);

    this.popover.removeEventListener('keydown', this.onKeyDown);
  }

  public open() {
    if (!this.popper) {
      this.popper = createPopper(
        this.button,
        this.popover,
        this.popoverOptions,
      );

      this.popover.classList.add('popover-menu--open');
      this.setExpanded(true);

      this.document.addEventListener('click', this.onDocumentClick);
    }
  }

  private onButtonClick = () => (this.popper ? this.close() : this.open());

  private onDocumentClick = (event: MouseEvent) => {
    const { defaultPrevented, target } = event;

    if (
      !defaultPrevented &&
      target instanceof Node &&
      !this.button.contains(target) &&
      !this.popover.contains(target)
    ) {
      this.close();
      event.preventDefault();
    }
  };

  private onKeyDown = (event: KeyboardEvent) => {
    const { key, shiftKey, target } = event;

    if (this.popper) {
      const shiftFocus = (offset: number) => {
        const items = Array.from(this.popover.getElementsByTagName('a'));

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
          this.button.focus();
          this.close();
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
      this.open();
      event.preventDefault();
    }
  };

  private setExpanded(expanded: boolean) {
    this.button.setAttribute('aria-expanded', expanded ? 'true' : 'false');
  }
}
