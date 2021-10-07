export default function recipesFilter(
  filterInput: HTMLInputElement, table: HTMLTableElement) {
  const rows: { text: string, element: Element }[] = [];

  table.querySelectorAll('tbody > tr').forEach(element => rows.push({
    element,
    text: element.firstElementChild.textContent.toLocaleLowerCase(),
  }));

  filterInput.addEventListener('input', apply);

  apply();

  function apply() {
    const tokens = filterInput.value.toLocaleLowerCase().split(/\s+/);
    rows.forEach(row => row.element.classList.toggle(
      'recipes-index--hidden', !tokens.every(token => row.text.includes(token))));
  }
}
