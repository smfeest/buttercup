import PopoverMenu from './popover-menu';

initializeTopBar();

function initializeTopBar() {
  const menuButton = document.getElementById('top-bar__menu-button');

  if (menuButton) {
    new PopoverMenu(
      document,
      menuButton,
      document.getElementById('top-bar__menu-popover'),
      { placement: 'bottom-end' },
    );
  }
}
