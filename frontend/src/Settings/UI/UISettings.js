import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';

export const firstDayOfWeekOptions = [
  { key: 0, value: translate('Sunday') },
  { key: 1, value: translate('Monday') }
];

export const weekColumnOptions = [
  { key: 'ddd M/D', value: 'Tue 3/25' },
  { key: 'ddd MM/DD', value: 'Tue 03/25' },
  { key: 'ddd D/M', value: 'Tue 25/3' },
  { key: 'ddd DD/MM', value: 'Tue 25/03' }
];

const shortDateFormatOptions = [
  { key: 'MMM D YYYY', value: 'Mar 25 2014' },
  { key: 'DD MMM YYYY', value: '25 Mar 2014' },
  { key: 'MM/D/YYYY', value: '03/25/2014' },
  { key: 'MM/DD/YYYY', value: '03/25/2014' },
  { key: 'DD/MM/YYYY', value: '25/03/2014' },
  { key: 'YYYY-MM-DD', value: '2014-03-25' }
];

const longDateFormatOptions = [
  { key: 'dddd, MMMM D YYYY', value: 'Tuesday, March 25, 2014' },
  { key: 'dddd, D MMMM YYYY', value: 'Tuesday, 25 March, 2014' }
];

export const timeFormatOptions = [
  { key: 'h(:mm)a', value: '5pm/5:30pm' },
  { key: 'HH:mm', value: '17:00/17:30' }
];

export const movieRuntimeFormatOptions = [
  { key: 'hoursMinutes', value: '1h 15m' },
  { key: 'minutes', value: '75 mins' }
];

class UISettings extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      settings,
      hasSettings,
      onInputChange,
      onSavePress,
      languages,
      ...otherProps
    } = this.props;

    const uiLanguages = languages.filter((item) => item.value !== 'Original');

    return (
      <PageContent title={translate('UISettings')}>
        <SettingsToolbarConnector
          {...otherProps}
          onSavePress={onSavePress}
        />

        <PageContentBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && error &&
              <div>
                {translate('UnableToLoadUISettings')}
              </div>
          }

          {
            hasSettings && !isFetching && !error &&
              <Form
                id="uiSettings"
                {...otherProps}
              >
                <FieldSet legend={translate('Calendar')}>
                  <FormGroup>
                    <FormLabel>{translate('SettingsFirstDayOfWeek')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="firstDayOfWeek"
                      values={firstDayOfWeekOptions}
                      onChange={onInputChange}
                      {...settings.firstDayOfWeek}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('SettingsWeekColumnHeader')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="calendarWeekColumnHeader"
                      values={weekColumnOptions}
                      onChange={onInputChange}
                      helpText={translate('SettingsWeekColumnHeaderHelpText')}
                      {...settings.calendarWeekColumnHeader}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet legend={translate('Movies')}>
                  <FormGroup>
                    <FormLabel>{translate('SettingsRuntimeFormat')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="movieRuntimeFormat"
                      values={movieRuntimeFormatOptions}
                      onChange={onInputChange}
                      {...settings.movieRuntimeFormat}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet legend={translate('Dates')}>
                  <FormGroup>
                    <FormLabel>{translate('SettingsShortDateFormat')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="shortDateFormat"
                      values={shortDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.shortDateFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('SettingsLongDateFormat')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="longDateFormat"
                      values={longDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.longDateFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('SettingsTimeFormat')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="timeFormat"
                      values={timeFormatOptions}
                      onChange={onInputChange}
                      {...settings.timeFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('SettingsShowRelativeDates')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="showRelativeDates"
                      helpText={translate('SettingsShowRelativeDatesHelpText')}
                      onChange={onInputChange}
                      {...settings.showRelativeDates}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet legend={translate('Style')}>
                  <FormGroup>
                    <FormLabel>{translate('SettingsEnableColorImpairedMode')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="enableColorImpairedMode"
                      helpText={translate('SettingsEnableColorImpairedModeHelpText')}
                      onChange={onInputChange}
                      {...settings.enableColorImpairedMode}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet legend={translate('Language')}>
                  <FormGroup>
                    <FormLabel>{translate('MovieInfoLanguage')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.LANGUAGE_SELECT}
                      name="movieInfoLanguage"
                      values={languages}
                      helpText={translate('MovieInfoLanguageHelpText')}
                      helpTextWarning={translate('MovieInfoLanguageHelpTextWarning')}
                      onChange={onInputChange}
                      {...settings.movieInfoLanguage}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('UILanguage')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.LANGUAGE_SELECT}
                      name="uiLanguage"
                      values={uiLanguages}
                      helpText={translate('UILanguageHelpText')}
                      helpTextWarning={translate('UILanguageHelpTextWarning')}
                      onChange={onInputChange}
                      {...settings.uiLanguage}
                    />
                  </FormGroup>
                </FieldSet>
              </Form>
          }
        </PageContentBody>
      </PageContent>
    );
  }

}

UISettings.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onSavePress: PropTypes.func.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UISettings;
