package com.pstehlik.groovy.gelf4j

import com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
import org.apache.log4j.spi.LoggingEvent
import org.apache.log4j.Category
import org.apache.log4j.Priority

/**
 * Test runner to send messages to graylog2 server
 *
 * @author Philip Stehlik
 * @since 0.8
 */
class Runner {
  public static void main(String[] args){
    def appender = new Gelf4JAppender()
    def logEvent = new LoggingEvent(Runner.class.name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Short Message", new Exception('Exception Message'))
    appender.graylogServerHost ='localhost'
    appender.append(logEvent)

    //set fields
    appender.with {
      additionalFields =['special':'hot', 'price':'free']
      facility = 0
      host = "pstehlik-localhost"
      includeLocationInformation = true
    }
    def otherLogEvent = new LoggingEvent(Runner.class.name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Other Short Message", new Exception('Exception Message'))
    appender.graylogServerHost ='ec2-184-73-68-116.compute-1.amazonaws.com'
    appender.append(otherLogEvent)

    appender.maxChunkSize = 160
    def longLogEvent = new LoggingEvent(Runner.class.name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, longChunkedMessage, new Exception('Exception Message'))
    appender.append(longLogEvent)
  }

  private static longChunkedMessage = """
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html
xmlns="http://www.w3.org/1999/xhtml"
lang="en" >
  <head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>pstehlik.com</title>

    <!-- Framework CSS -->
    <link rel="stylesheet" href="blueprint/screen.css" type="text/css" media="screen, projection" />
    <link rel="stylesheet" href="blueprint/print.css" type="text/css" media="print" />
    <!--[if lt IE 8]><link rel="stylesheet" href="blueprint/ie.css" type="text/css" media="screen, projection" /><![endif]-->

    <!-- Import fancy-type plugin -->
    <link rel="stylesheet" href="blueprint/plugins/fancy-type/screen.css" type="text/css" media="screen, projection" />

    <!-- Personal style fixes -->
    <link rel="stylesheet" href="css/pstehlik.css" type="text/css" media="screen, projection" />

    <!-- Google Tracking -->
    <script type="text/javascript">
      var _gaq = _gaq || [];
      _gaq.push(['_setAccount', 'UA-5902277-6']);
      _gaq.push(['_setDomainName', '.pstehlik.com']);
      _gaq.push(['_trackPageview']);

      (function() {
        var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
        ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
      })();
    </script>
  </head>
  <body>
    <div class="container">
      <hr class="space" />
      <h1>Luckily it is a neverending story</h1>
      <hr />
      <h2 class="alt">Welcome to the internet-home of Philip Stehlik</h2>
      <hr />
      <div class="span-7 colborder">
        <h6 class="link"><a href="http://www.taulia.com/page/invoicement?utm_campaign=pstehlik&amp;utm_medium=pstehlik.com&amp;utm_term=my_company">My Company</a></h6>
        <p>At <a href="http://www.taulia.com/page/invoicement?utm_campaign=pstehlik&amp;utm_medium=pstehlik.com&amp;utm_term=taulia">Taulia</a> we enable and incentivize large corporations to pay their invoices early.  We use pretty advanced technology for that, are completely 'in the cloud' and connect on-premise solutions and third parties to our platform for easy and secure transactions.  We are also most probably <a href="http://www.taulia.com/page/careers?utm_campaign=pstehlik&amp;utm_medium=pstehlik.com&amp;utm_source=&amp;utm_content=&amp;utm_term=">hiring</a> right now.</p>
      </div>
      <div class="span-8 colborder">
        <h6 class="link"><a href="http://www.softwareinsane.com?utm_campaign=pstehlik&amp;utm_medium=pstehlik.com&amp;utm_term=technology">Technology</a></h6>
        <p>At <a href="http://www.softwareinsane.com?utm_campaign=pstehlik&amp;utm_medium=pstehlik.com&amp;utm_term=softwareinsane">softwareinsane.com</a> I write about my experiences with using and abusing software and the tools I use for that.  Mainly about coding, Groovy, Grails, Java, AWS and SAP.<br/><br/><br/><br/><br/></p>
      </div>
      <div class="span-7 last">
        <h6 class="link"><a href="http://thinkoutsidethebubble.net?utm_campaign=pstehlik&amp;utm_medium=pstehlik.com&amp;utm_term=entrepreneur_stuff">Entrepreneur Stuff</a></h6>
        <p>I share thoughts, ideas and personal opinions at <a href="http://thinkoutsidethebubble.net?utm_campaign=pstehlik&amp;utm_medium=pstehlik.com&amp;utm_term=thinkoutsidethebubble">thinkoutsidethebubble.net</a> where I write about anything that matters to me at the time of writing.  No special topic or audience.  Maybe when I am over 30 and start forgetting things I will use that blog to remember things.<br/><br/></p>
      </div>
      <hr />
      <div class="whiteText">
        <h6>Things I have on my todo list</h6>
        <ul>
          <li>Hack my cable modem to give me more speed</li>
          <li>Rewrite getFooter in scala or with roo</li>
          <li>Relearn Spanish</li>
          <li>Live in Asia and learn an asian language</li>
          <li>Complete my training to the 1st Dan in Jiu Jitsu</li>
          <li>Design and build the "Mother of all status boards" for display of system and codebase status</li>
          <li>Take a photography class</li>
          <li>Build my own guitar</li>
          <li>Learn a backflip on the snowboad</li>
          <li>Film the movie I am collecting scripts for</li>
          <li>Start a <a href="http://en.wikipedia.org/wiki/Doner_kebab">Döner Kebap</a> restaurant in SF</li>
          <li>...</li>
        </ul>
      </div>
    </div>
  </body>
</html> """
}
