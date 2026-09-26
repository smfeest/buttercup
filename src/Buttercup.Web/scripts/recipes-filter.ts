export default (filterInput: HTMLInputElement, list: HTMLUListElement) => {
  const rows: { text: string; element: Element }[] = [];

  const apply = () => {
    const tokens = filterInput.value.toLocaleLowerCase().split(/\s+/);
    rows.forEach(({ element, text }) =>
      element.classList.toggle(
        'recipes-index--hidden',
        !tokens.every((token) => text.includes(token)),
      ),
    );
  };

  list.querySelectorAll('li').forEach((element) =>
    rows.push({
      element,
      text: element.textContent.toLocaleLowerCase(),
    }),
  );

  filterInput.addEventListener('input', apply);

  apply();
};
