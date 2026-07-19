import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import { setPersonalizedUiValue } from 'Store/Actions/personalizedUiActions';
import { normalizeAccentColor } from 'Personalization/preferences';
import createPersonalizedUiSelector from 'Store/Selectors/createPersonalizedUiSelector';

const options = (values: string[]) =>
  values.map((value) => ({
    key: value,
    value: value.charAt(0).toUpperCase() + value.slice(1),
  }));

function AppearanceSettings() {
  const dispatch = useDispatch();
  const preferences = useSelector(createPersonalizedUiSelector());
  const handleChange = useCallback(
    ({ name, value }: { name: string; value: string | boolean }) => {
      dispatch(setPersonalizedUiValue({ [name]: name === 'accentColor' ? normalizeAccentColor(String(value)) : value }));
    },
    [dispatch]
  );

  return (
    <FieldSet legend="Appearance">
      <FormGroup>
        <FormLabel>Theme</FormLabel>
        <FormInputGroup type={inputTypes.SELECT} name="theme" value={preferences.theme} values={options(['system', 'light', 'dark', 'oled'])} onChange={handleChange} />
      </FormGroup>
      <FormGroup>
        <FormLabel>Accent color</FormLabel>
        <FormInputGroup type={inputTypes.TEXT} name="accentColor" value={preferences.accentColor} helpText="CSS color used for focus, selection, and active navigation." onChange={handleChange} />
      </FormGroup>
      <FormGroup>
        <FormLabel>Interface density</FormLabel>
        <FormInputGroup type={inputTypes.SELECT} name="density" value={preferences.density} values={options(['compact', 'comfortable', 'spacious'])} onChange={handleChange} />
      </FormGroup>
      <FormGroup>
        <FormLabel>Poster size</FormLabel>
        <FormInputGroup type={inputTypes.SELECT} name="posterSize" value={preferences.posterSize} values={options(['small', 'medium', 'large'])} onChange={handleChange} />
      </FormGroup>
      <FormGroup>
        <FormLabel>Card style</FormLabel>
        <FormInputGroup type={inputTypes.SELECT} name="cardStyle" value={preferences.cardStyle} values={options(['rounded', 'square'])} onChange={handleChange} />
      </FormGroup>
      <FormGroup>
        <FormLabel>Interface animations</FormLabel>
        <FormInputGroup type={inputTypes.CHECK} name="enableAnimations" value={preferences.enableAnimations} onChange={handleChange} />
      </FormGroup>
      <FormGroup>
        <FormLabel>Movie backdrop backgrounds</FormLabel>
        <FormInputGroup type={inputTypes.CHECK} name="enableBackdrops" value={preferences.enableBackdrops} helpText="Allows movie pages and cards to use available backdrop artwork." onChange={handleChange} />
      </FormGroup>
    </FieldSet>
  );
}

export default AppearanceSettings;
