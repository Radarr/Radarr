import { reduce } from 'lodash';
import { SelectedState } from 'Helpers/Hooks/useSelectState';

function getSelectedIds(selectedState: SelectedState): number[] {
  return reduce(
    selectedState,
    (result: number[], value, id) => {
      if (value) {
        const parsed = Number.parseInt(id, 10);
        if (!Number.isNaN(parsed)) {
          result.push(parsed);
        }
      }

      return result;
    },
    []
  );
}

export default getSelectedIds;
