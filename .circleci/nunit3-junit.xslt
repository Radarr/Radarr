<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/test-run">
    <testsuites tests="{@testcasecount}" failures="{@failed}" disabled="{@skipped}" time="{@duration}">
      <xsl:apply-templates/>
    </testsuites>
  </xsl:template>

  <xsl:template match="test-suite">
    <xsl:if test="test-case">
      <testsuite tests="{@testcasecount}" time="{@duration}" errors="{@testcasecount - @passed - @skipped - @failed}" failures="{@failed}" skipped="{@skipped}" timestamp="{@start-time}">
        <xsl:attribute name="name">
          <xsl:for-each select="ancestor-or-self::test-suite/@name">
            <xsl:value-of select="concat(., '.')"/>
          </xsl:for-each>
        </xsl:attribute>
        <xsl:apply-templates select="test-case"/>
      </testsuite>
      <xsl:apply-templates select="test-suite"/>
    </xsl:if>
    <xsl:if test="not(test-case)">
      <xsl:apply-templates/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="test-case">
    <testcase name="{@name}" assertions="{@asserts}" time="{@duration}" status="{@result}" classname="{@classname}">
      <xsl:if test="@runstate = 'Skipped' or @runstate = 'Ignored'">
        <skipped/>
      </xsl:if>
      
      <xsl:apply-templates/>
    </testcase>
  </xsl:template>

  <xsl:template match="command-line"/>
  <xsl:template match="settings"/>

  <xsl:template match="output">
    <system-out>
      <xsl:value-of select="."/>
    </system-out>
  </xsl:template>

  <xsl:template match="stack-trace">
  </xsl:template>

  <xsl:template match="test-case/failure">
    <failure message="{./message}">
      <xsl:value-of select="./stack-trace"/>
    </failure>
  </xsl:template>

  <xsl:template match="test-suite/failure"/>

  <xsl:template match="test-case/reason">
    <skipped message="{./message}"/>
  </xsl:template>
  
  <xsl:template match="test-case/assertions">
  </xsl:template>

  <xsl:template match="test-suite/reason"/>

  <xsl:template match="properties"/>
</xsl:stylesheet>

