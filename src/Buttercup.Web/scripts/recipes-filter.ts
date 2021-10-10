export default (filterInput: HTMLInputElement, table: HTMLTableElement) => {
  const rows: { text: string; element: Element }[] = [];

  const apply = () => {
    const tokens = filterInput.value.toLocaleLowerCase().split(/\s+/);
    rows.forEach(({ element, text }) =>
      element.classList.toggle(
        'recipes-index--hidden',
        !tokens.every((token) => text.includes(token))
      )
    );
  };

  table.querySelectorAll('tbody > tr').forEach((element) =>
    rows.push({
      element,
      text: element.firstElementChild!.textContent!.toLocaleLowerCase(),
    })
  );

  filterInput.addEventListener('input', apply);

  apply();
};
