import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactSlider from 'react-slider';
import formatBytes from 'Utilities/Number/formatBytes';
import roundNumber from 'Utilities/Number/roundNumber';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import NumberInput from 'Components/Form/NumberInput';
import TextInput from 'Components/Form/TextInput';
import styles from './QualityDefinition.css';

const MIN = 0;
const MAX = 400;

const slider = {
  min: MIN,
  max: roundNumber(Math.pow(MAX, 1 / 1.1)),
  step: 0.1
};

function getValue(inputValue) {
  if (inputValue < MIN) {
    return MIN;
  }

  if (inputValue > MAX) {
    return MAX;
  }

  return roundNumber(inputValue);
}

function getSliderValue(value, defaultValue) {
  const sliderValue = value ? Math.pow(value, 1 / 1.1) : defaultValue;

  return roundNumber(sliderValue);
}

class QualityDefinition extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._forceUpdateTimeout = null;

    this.state = {
      sliderMinSize: getSliderValue(props.minSize, slider.min),
      sliderMaxSize: getSliderValue(props.maxSize, slider.max)
    };
  }

  componentDidMount() {
    // A hack to deal with a bug in the slider component until a fix for it
    // lands and an updated version is available.
    // See: https://github.com/mpowaga/react-slider/issues/115

    this._forceUpdateTimeout = setTimeout(() => this.forceUpdate(), 1);
  }

  componentWillUnmount() {
    if (this._forceUpdateTimeout) {
      clearTimeout(this._forceUpdateTimeout);
    }
  }

  //
  // Listeners

  onSliderChange = ([sliderMinSize, sliderMaxSize]) => {
    this.setState({
      sliderMinSize,
      sliderMaxSize
    });

    this.props.onSizeChange({
      minSize: roundNumber(Math.pow(sliderMinSize, 1.1)),
      maxSize: sliderMaxSize === slider.max ? null : roundNumber(Math.pow(sliderMaxSize, 1.1))
    });
  }

  onAfterSliderChange = () => {
    const {
      minSize,
      maxSize
    } = this.props;

    this.setState({
      sliderMiSize: getSliderValue(minSize, slider.min),
      sliderMaxSize: getSliderValue(maxSize, slider.max)
    });
  }

  onMinSizeChange = ({ value }) => {
    const minSize = getValue(value);

    this.setState({
      sliderMinSize: getSliderValue(minSize, slider.min)
    });

    this.props.onSizeChange({
      minSize,
      maxSize: this.props.maxSize
    });
  }

  onMaxSizeChange = ({ value }) => {
    const maxSize = value === MAX ? null : getValue(value);

    this.setState({
      sliderMaxSize: getSliderValue(maxSize, slider.max)
    });

    this.props.onSizeChange({
      minSize: this.props.minSize,
      maxSize
    });
  }

  //
  // Render

  render() {
    const {
      id,
      quality,
      title,
      minSize,
      maxSize,
      advancedSettings,
      onTitleChange
    } = this.props;

    const {
      sliderMinSize,
      sliderMaxSize
    } = this.state;

    const minBytes = minSize * 1024 * 1024;
    const minThirty = formatBytes(minBytes * 90, 2);
    const minSixty = formatBytes(minBytes * 140, 2);

    const maxBytes = maxSize && maxSize * 1024 * 1024;
    const maxThirty = maxBytes ? formatBytes(maxBytes * 90, 2) : 'Unlimited';
    const maxSixty = maxBytes ? formatBytes(maxBytes * 140, 2) : 'Unlimited';

    return (
      <div className={styles.qualityDefinition}>
        <div className={styles.quality}>
          {quality.name}
        </div>

        <div className={styles.title}>
          <TextInput
            name={`${id}.${title}`}
            value={title}
            onChange={onTitleChange}
          />
        </div>

        <div className={styles.sizeLimit}>
          <ReactSlider
            min={slider.min}
            max={slider.max}
            step={slider.step}
            minDistance={10}
            value={[sliderMinSize, sliderMaxSize]}
            withBars={true}
            snapDragDisabled={true}
            className={styles.slider}
            barClassName={styles.bar}
            handleClassName={styles.handle}
            onChange={this.onSliderChange}
            onAfterChange={this.onAfterSliderChange}
          />

          <div className={styles.sizes}>
            <div>
              <Label kind={kinds.WARNING}>{minThirty}</Label>
              <Label kind={kinds.INFO}>{minSixty}</Label>
            </div>

            <div>
              <Label kind={kinds.WARNING}>{maxThirty}</Label>
              <Label kind={kinds.INFO}>{maxSixty}</Label>
            </div>
          </div>
        </div>

        {
          advancedSettings &&
            <div className={styles.megabytesPerMinute}>
              <div>
                Min

                <NumberInput
                  className={styles.sizeInput}
                  name={`${id}.min`}
                  value={minSize || MIN}
                  min={MIN}
                  max={maxSize ? maxSize - 10 : MAX - 10}
                  step={0.1}
                  isFloat={true}
                  onChange={this.onMinSizeChange}
                />
              </div>

              <div>
                Max

                <NumberInput
                  className={styles.sizeInput}
                  name={`${id}.min`}
                  value={maxSize || MAX}
                  min={minSize + 10}
                  max={MAX}
                  step={0.1}
                  isFloat={true}
                  onChange={this.onMaxSizeChange}
                />
              </div>
            </div>
        }
      </div>
    );
  }
}

QualityDefinition.propTypes = {
  id: PropTypes.number.isRequired,
  quality: PropTypes.object.isRequired,
  title: PropTypes.string.isRequired,
  minSize: PropTypes.number,
  maxSize: PropTypes.number,
  advancedSettings: PropTypes.bool.isRequired,
  onTitleChange: PropTypes.func.isRequired,
  onSizeChange: PropTypes.func.isRequired
};

export default QualityDefinition;
