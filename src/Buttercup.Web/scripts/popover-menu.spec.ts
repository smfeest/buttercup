import PopoverMenu, { Options } from './popover-menu';

describe('PopoverMenu', () => {
  let fixture: HTMLElement;
  let button: HTMLButtonElement;
  let popover: HTMLElement;

  let popoverMenu: PopoverMenu;

  beforeEach(() => {
    document.body.appendChild((fixture = document.createElement('div')));
    fixture.appendChild((button = document.createElement('button')));
    fixture.appendChild((popover = document.createElement('div')));
  });

  afterEach(() => {
    if (popoverMenu) {
      popoverMenu.destroy();
    }

    fixture.remove();
  });

  const initializePopoverMenu = (popoverOptions?: Options) =>
    (popoverMenu = new PopoverMenu(document, button, popover, popoverOptions));

  const addMenuItem = () => {
    const menuItem = document.createElement('a');
    menuItem.href = '#';

    popover.appendChild(menuItem);

    return menuItem;
  };

  const triggerClick = (target: HTMLElement) => {
    const event = new MouseEvent('click', { bubbles: true });
    spyOn(event, 'preventDefault');

    target.dispatchEvent(event);

    return event;
  };

  const triggerKeyDown = (key: string, properties?: KeyboardEventInit) => {
    const event = new KeyboardEvent('keydown', {
      key,
      bubbles: true,
      ...properties,
    });
    spyOn(event, 'preventDefault');

    document.activeElement!.dispatchEvent(event);

    return event;
  };

  describe('initialization', () => {
    it("generates a unique ID for the button, if it doesn't already have one", () => {
      for (let i = 0; i < 2; i++) {
        const otherButton = document.createElement('button');
        otherButton.id = `popover-menu-button-${i}`;
        fixture.appendChild(otherButton);
      }

      initializePopoverMenu();

      expect(button.id).toEqual('popover-menu-button-2');
    });

    it('does not generate a new ID for the button if it already has one', () => {
      button.id = 'sample-button-id';

      initializePopoverMenu();

      expect(button.id).toEqual('sample-button-id');
    });

    it('adds the `aria-haspopup` attribute to the button', () => {
      initializePopoverMenu();

      expect(button.getAttribute('aria-haspopup')).toEqual('true');
    });

    it('adds the `aria-expanded` attribute to the button', () => {
      initializePopoverMenu();

      expect(button.getAttribute('aria-expanded')).toEqual('false');
    });

    it('adds the `aria-labelledby` attribute to the button', () => {
      button.id = 'sample-button-id';

      initializePopoverMenu();

      expect(popover.getAttribute('aria-labelledby')).toEqual(
        'sample-button-id'
      );
    });

    it('sets `isOpen` to false', () => {
      initializePopoverMenu();

      expect(popoverMenu.isOpen).toBe(false);
    });
  });

  describe('close()', () => {
    beforeEach(() => {
      initializePopoverMenu().open();
    });

    it('sets the `aria-expanded` attribute on the button to false', () => {
      popoverMenu.close();

      expect(button.getAttribute('aria-expanded')).toEqual('false');
    });

    it('removes the open modifier class from the popover', () => {
      popoverMenu.close();

      expect(popover.classList.contains('popover-menu--open')).toBe(false);
    });

    it('stops positioning the popover', async () => {
      popoverMenu.close();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(popover.hasAttribute('data-popper-placement')).toBe(false);
    });

    it('sets `isOpen` to false', () => {
      popoverMenu.close();

      expect(popoverMenu.isOpen).toBe(false);
    });
  });

  describe('destroy()', () => {
    beforeEach(() => initializePopoverMenu().open());

    it('closes the popover', () => {
      popoverMenu.destroy();

      expect(popoverMenu.isOpen).toBe(false);
    });

    it('removes the button click handler', () => {
      popoverMenu.destroy();

      triggerClick(button);

      expect(popoverMenu.isOpen).toBe(false);
    });

    it('removes the button key down handler', () => {
      popoverMenu.destroy();

      button.focus();
      triggerKeyDown('ArrowDown');

      expect(popoverMenu.isOpen).toBe(false);
    });

    it('removes the popover key down handler', () => {
      popoverMenu.destroy();

      addMenuItem().focus();
      triggerKeyDown('ArrowDown');

      expect(popoverMenu.isOpen).toBe(false);
    });
  });

  describe('open()', () => {
    it('sets the `aria-expanded` attribute on the button to true', () => {
      initializePopoverMenu().open();

      expect(button.getAttribute('aria-expanded')).toEqual('true');
    });

    it('adds the open modifier class to the popover', () => {
      initializePopoverMenu().open();

      expect(popover.classList.contains('popover-menu--open')).toBe(true);
    });

    it('positions the popover with the specified placement', async () => {
      initializePopoverMenu({ placement: 'left-start' }).open();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(popover.getAttribute('data-popper-placement')).toEqual(
        'left-start'
      );
    });

    it('sets `isOpen` to true', () => {
      initializePopoverMenu().open();

      expect(popoverMenu.isOpen).toBe(true);
    });
  });

  describe('when closed', () => {
    beforeEach(() => initializePopoverMenu());

    describe('clicking the button', () => {
      it('opens the popover', () => {
        triggerClick(button);

        expect(popoverMenu.isOpen).toBe(true);
      });
    });

    describe('when focus is on button', () => {
      beforeEach(() => button.focus());

      describe('pressing down key', () => {
        it('opens the popover', () => {
          triggerKeyDown('ArrowDown');

          expect(popoverMenu.isOpen).toBe(true);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('ArrowDown');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing up key', () => {
        it('opens the popover', () => {
          triggerKeyDown('ArrowUp');

          expect(popoverMenu.isOpen).toBe(true);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('ArrowDown');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });
    });
  });

  describe('when open', () => {
    let menuItems: HTMLAnchorElement[];

    beforeEach(() => {
      menuItems = [];

      for (let i = 0; i < 3; i++) {
        menuItems.push(addMenuItem());
      }

      initializePopoverMenu().open();
    });

    describe('clicking the button', () => {
      it('closes the popover', () => {
        triggerClick(button);

        expect(popoverMenu.isOpen).toBe(false);
      });
    });

    describe('clicking outside the popover', () => {
      it('closes the popover', () => {
        triggerClick(fixture);

        expect(popoverMenu.isOpen).toBe(false);
      });

      it('suppresses the default click behaviour', () => {
        const event = triggerClick(fixture);

        expect(event.preventDefault).toHaveBeenCalled();
      });
    });

    describe('clicking inside the popover', () => {
      it('does not close the popover', () => {
        triggerClick(menuItems[0]);

        expect(popoverMenu.isOpen).toBe(true);
      });

      it('does not suppress the default click behaviour', () => {
        const event = triggerClick(menuItems[0]);

        expect(event.preventDefault).not.toHaveBeenCalled();
      });
    });

    describe('when focus is on button', () => {
      beforeEach(() => button.focus());

      describe('pressing down key', () => {
        it('sets focus on the first menu item', () => {
          triggerKeyDown('ArrowDown');

          expect(document.activeElement).toBe(menuItems[0]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('ArrowDown');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing up key', () => {
        it('sets focus on the last menu item', () => {
          triggerKeyDown('ArrowUp');

          expect(document.activeElement).toBe(menuItems[2]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('ArrowDown');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing tab key', () => {
        it('sets focus on the first menu item', () => {
          triggerKeyDown('Tab');

          expect(document.activeElement).toBe(menuItems[0]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('Tab');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing tab key with shift', () => {
        it('sets focus on the last menu item', () => {
          triggerKeyDown('Tab', { shiftKey: true });

          expect(document.activeElement).toBe(menuItems[2]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('Tab', { shiftKey: true });

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing escape key', () => {
        it('closes the popover', () => {
          triggerKeyDown('Escape');

          expect(popoverMenu.isOpen).toBe(false);
        });

        it('sets focus on the button', () => {
          triggerKeyDown('Escape');

          expect(document.activeElement).toBe(button);
        });
      });
    });

    describe('when focus is on a menu item', () => {
      beforeEach(() => menuItems[1].focus());

      describe('pressing down key', () => {
        it('sets focus on the next menu item', () => {
          triggerKeyDown('ArrowDown');

          expect(document.activeElement).toBe(menuItems[2]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('ArrowDown');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing up key', () => {
        it('sets focus on the previous menu item', () => {
          triggerKeyDown('ArrowUp');

          expect(document.activeElement).toBe(menuItems[0]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('ArrowDown');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing tab key', () => {
        it('sets focus on the next menu item', () => {
          triggerKeyDown('Tab');

          expect(document.activeElement).toBe(menuItems[2]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('Tab');

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing tab key with shift', () => {
        it('sets focus on the previous menu item', () => {
          triggerKeyDown('Tab', { shiftKey: true });

          expect(document.activeElement).toBe(menuItems[0]);
        });

        it('suppresses the default key down behaviour', () => {
          const event = triggerKeyDown('Tab', { shiftKey: true });

          expect(event.preventDefault).toHaveBeenCalled();
        });
      });

      describe('pressing escape key', () => {
        it('closes the popover', () => {
          triggerKeyDown('Escape');

          expect(popoverMenu.isOpen).toBe(false);
        });

        it('sets focus on the button', () => {
          triggerKeyDown('Escape');

          expect(document.activeElement).toBe(button);
        });
      });
    });

    describe('when focus is on first menu item', () => {
      beforeEach(() => menuItems[0].focus());

      describe('pressing up key', () => {
        it('sets focus on the last menu item', () => {
          triggerKeyDown('ArrowUp');

          expect(document.activeElement).toBe(menuItems[2]);
        });
      });

      describe('pressing tab key with shift', () => {
        it('sets focus on the last menu item', () => {
          triggerKeyDown('Tab', { shiftKey: true });

          expect(document.activeElement).toBe(menuItems[2]);
        });
      });
    });

    describe('when focus is on last menu item', () => {
      beforeEach(() => menuItems[2].focus());

      describe('pressing down key', () => {
        it('sets focus on the first menu item', () => {
          triggerKeyDown('ArrowDown');

          expect(document.activeElement).toBe(menuItems[0]);
        });
      });

      describe('pressing tab key', () => {
        it('sets focus on the first menu item', () => {
          triggerKeyDown('Tab');

          expect(document.activeElement).toBe(menuItems[0]);
        });
      });
    });
  });
});
