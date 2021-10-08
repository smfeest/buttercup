import Popper, { PopperOptions } from 'popper.js';

export default class PopoverMenu {
  private _isOpen = false;
  private popper?: Popper;

  public constructor(
    public document: Document,
    public button: HTMLElement,
    public popover: HTMLElement,
    public popoverOptions?: PopperOptions) {
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
    return this._isOpen;
  }

  public close() {
    if (this._isOpen) {
      this.popover.classList.remove('popover-menu--open');
      this.setExpanded(false);

      this.popper!.destroy();
      this.popper = undefined;

      this.document.removeEventListener('click', this.onDocumentClick);

      this._isOpen = false;
    }
  }

  public destroy() {
    this.close();

    this.button.removeEventListener('click', this.onButtonClick);
    this.button.removeEventListener('keydown', this.onKeyDown);

    this.popover.removeEventListener('keydown', this.onKeyDown);
  }

  public open() {
    if (!this._isOpen) {
      this.popper = new Popper(this.button, this.popover, this.popoverOptions);

      this.popover.classList.add('popover-menu--open');
      this.setExpanded(true);

      this.document.addEventListener('click', this.onDocumentClick);

      this._isOpen = true;
    }
  }

  private onButtonClick = () => {
    if (this._isOpen) {
      this.close();
    } else {
      this.open();
    }
  }

  private onDocumentClick = (event: MouseEvent) => {
    if (!event.defaultPrevented &&
      event.target instanceof Node &&
      !this.button.contains(event.target) &&
      !this.popover.contains(event.target)) {
      this.close();
      event.preventDefault();
    }
  }

  private onKeyDown = (event: KeyboardEvent) => {
    if (this.isOpen) {
      const shiftFocus = (offset: number) => {
        const items = Array.from(this.popover.getElementsByTagName('a'));

        if (items.length > 0) {
          let targetIndex = items.indexOf(event.target as HTMLAnchorElement) + offset;
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

      switch (event.key) {
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
          shiftFocus(event.shiftKey ? -1 : 1);
          break;
      }
    } else if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      this.open();
      event.preventDefault();
    }
  }

  private setExpanded(expanded: boolean) {
    this.button.setAttribute('aria-expanded', expanded ? 'true' : 'false');
  }
}
