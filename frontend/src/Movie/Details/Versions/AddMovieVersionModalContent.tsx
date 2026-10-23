import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { inputTypes, kinds } from 'Helpers/Props';
import useMovie from 'Movie/useMovie';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import { InputChanged } from 'typings/inputs';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import translate from 'Utilities/String/translate';

type Location = 'movieFolder' | 'otherRootFolder';

interface MovieFolder {
  folder: string;
}

export interface AddMovieVersionModalContentProps {
  movieId: number;
  onVersionAdded(): void;
  onModalClose(): void;
}

function AddMovieVersionModalContent({
  movieId,
  onVersionAdded,
  onModalClose,
}: AddMovieVersionModalContentProps) {
  const movie = useMovie(movieId);
  const { isWindows } = useSelector(createSystemStatusSelector());

  const [qualityProfileId, setQualityProfileId] = useState(
    movie?.qualityProfileId ?? 0
  );
  const [location, setLocation] = useState<Location>('movieFolder');
  const [rootFolderPath, setRootFolderPath] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  const { data: movieFolder } = useApiQuery<MovieFolder>({
    path: `/movie/${movieId}/folder`,
  });

  const locationOptions = [
    {
      key: 'movieFolder',
      value: translate('MovieVersionLocationMovieFolder'),
    },
    {
      key: 'otherRootFolder',
      value: translate('MovieVersionLocationOtherRootFolder'),
    },
  ];

  const handleQualityProfileChange = useCallback(
    ({ value }: InputChanged<string | number>) => {
      setQualityProfileId(Number(value));
    },
    []
  );

  const handleLocationChange = useCallback(
    ({ value }: InputChanged<Location>) => {
      setLocation(value);
    },
    []
  );

  const handleRootFolderChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setRootFolderPath(value);
    },
    []
  );

  const handleAddPress = useCallback(() => {
    setIsSaving(true);
    setSaveError(null);

    const { request } = createAjaxRequest({
      url: '/movieversion',
      method: 'POST',
      dataType: 'json',
      data: JSON.stringify({
        movieId,
        qualityProfileId,
        rootFolderPath: location === 'otherRootFolder' ? rootFolderPath : null,
      }),
    });

    request
      .done(() => {
        setIsSaving(false);
        onVersionAdded();
      })
      .fail((xhr: { responseJSON?: { message?: string } }) => {
        setIsSaving(false);
        setSaveError(xhr.responseJSON?.message ?? translate('Error'));
      });
  }, [movieId, qualityProfileId, location, rootFolderPath, onVersionAdded]);

  const isRootFolderMissing = location === 'otherRootFolder' && !rootFolderPath;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddVersion')}</ModalHeader>

      <ModalBody>
        {saveError ? <Alert kind={kinds.DANGER}>{saveError}</Alert> : null}

        <Form>
          <FormGroup>
            <FormLabel>{translate('QualityProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.QUALITY_PROFILE_SELECT}
              name="qualityProfileId"
              value={qualityProfileId}
              helpText={translate('MovieVersionQualityProfileHelpText')}
              onChange={handleQualityProfileChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Location')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="location"
              value={location}
              values={locationOptions}
              helpText={translate('MovieVersionLocationHelpText')}
              onChange={handleLocationChange}
            />
          </FormGroup>

          {location === 'otherRootFolder' ? (
            <FormGroup>
              <FormLabel>{translate('RootFolder')}</FormLabel>

              <FormInputGroup
                type={inputTypes.ROOT_FOLDER_SELECT}
                name="rootFolderPath"
                value={rootFolderPath}
                valueOptions={{
                  movieFolder: movieFolder?.folder,
                  isWindows,
                }}
                selectedValueOptions={{
                  movieFolder: movieFolder?.folder,
                  isWindows,
                }}
                onChange={handleRootFolderChange}
              />
            </FormGroup>
          ) : null}
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerButton
          kind={kinds.PRIMARY}
          isSpinning={isSaving}
          isDisabled={isRootFolderMissing}
          onPress={handleAddPress}
        >
          {translate('Add')}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddMovieVersionModalContent;
